using QueueService;
using Repository;
using IRepository;
using Shared;
using db;
using Microsoft.AspNetCore.SignalR;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices();

var app = builder.Build();

app.MapHub<CurrencyRateHub>("/currencyRateHub");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/db/init", () =>
{
    DB db = new();
    db.Init();

    return Results.Ok();
});

app.MapGet("/customer/queue/clean", async (
    CustomerQueue queue) =>
{
    await queue.CleanQueue();

    return Results.Ok();
});

app.MapGet("/customer/queue", async (
    CustomerQueue queue) =>
{
    var value = await queue.EnqueueCustomer();

    return Results.Ok(value);
});

app.MapGet("/customer/next", async (
    int displayId,
    int tellerId,
    CustomerQueue queue,
    SocketServer socketServer) =>
{
    int? customerNumber =
        await queue.DequeueCustomer();

    if (customerNumber == null)
    {
        return Results.NotFound(
            "No customers waiting."
        );
    }

    await socketServer.SendCallAsync(
        displayId,
        tellerId,
        customerNumber.Value
    );

    return Results.Ok(new
    {
        tellerId,
        displayId,
        customerNumber = customerNumber.Value
    });
});

app.MapPost("/teller/login", async (
    TellerLoginRequest req,
    ITellerRepository tellerRepo) =>
{
    var teller = await tellerRepo.GetTeller(req.Name);

    if (teller == null)
        return Results.NotFound("No such teller");

    if (teller.Password != req.Password)
        return Results.Unauthorized();

    return Results.Ok(teller);
});

app.MapPost("/teller/add", async (
    TellerRegisterRequest req,
    ITellerRepository tellerRepo) =>
{
    await tellerRepo.AddTeller(
        req.Name,
        req.Password
    );

    return Results.Ok();
});

app.MapPost("/customer/add", async (
    AddCustomerRequest req,
    ICustomerRepository customerRepo) =>
{
    await customerRepo.AddCustomer(
        req.Name,
        req.RegistNumber
    );

    var customer = await customerRepo
        .GetCustomerByRegistNumber(req.RegistNumber);

    return Results.Ok(customer);
});

app.MapPost("/customer/account/add", async (
    AddAccountRequest req,
    ICustomerRepository customerRepo,
    IAccountRepository accountRepo) =>
{
    var customer = await customerRepo
        .GetCustomerByCustomerId(req.CustomerId);

    if (customer == null)
        return Results.BadRequest("No such customer");

    await accountRepo.AddAccount(
        customer.Id,
        req.AccountType
    );

    return Results.Ok();
});

app.MapGet("/customer/{registerNumber}", async (
    string registerNumber,
    ICustomerRepository customerRepo,
    IAccountRepository accountRepo) =>
{
    var customer = await customerRepo
        .GetCustomerByRegistNumber(registerNumber);

    if (customer == null)
        return Results.NotFound("Customer not found");

    var accounts = await accountRepo
        .GetAccountsByCustomerId(customer.Id);

    return Results.Ok(new CustomerInfoResponse(
        customer.Id,
        customer.Name,
        customer.RegistNumber,
        accounts
    ));
});

app.MapPost("/account/withdraw", async (
    ChangeBalanceRequest req,
    IAccountRepository accountRepo) =>
{
    if (req.Amount <= 0)
        return Results.BadRequest(
            "Withdraw amount must be greater than 0"
        );

    var account = await accountRepo
        .GetAccount(req.AccountNumber);

    if (account == null)
        return Results.NotFound("Account not found");

    if (account.CustomerId != req.CustomerId)
        return Results.BadRequest(
            "Account does not belong to this customer"
        );

    var newBalance = account.Balance - req.Amount;

    if (newBalance < 0)
        return Results.BadRequest("Not enough balance");

    await accountRepo.UpdateBalance(
        account.CustomerId,
        account.AccountNumber,
        newBalance
    );

    return Results.Ok(new
    {
        account.AccountNumber,
        OldBalance = account.Balance,
        NewBalance = newBalance
    });
});

app.MapPost("/account/deposit", async (
    ChangeBalanceRequest req,
    IAccountRepository accountRepo) =>
{
    if (req.Amount <= 0)
        return Results.BadRequest(
            "Deposit amount must be greater than 0"
        );

    var account = await accountRepo
        .GetAccount(req.AccountNumber);

    if (account == null)
        return Results.NotFound("Account not found");

    if (account.CustomerId != req.CustomerId)
        return Results.BadRequest(
            "Account does not belong to this customer"
        );

    var newBalance = account.Balance + req.Amount;

    await accountRepo.UpdateBalance(
        account.CustomerId,
        account.AccountNumber,
        newBalance
    );

    return Results.Ok(new
    {
        account.AccountNumber,
        OldBalance = account.Balance,
        NewBalance = newBalance
    });
});

app.MapPost("/account/transfer/{targetAccountNumber}", async (
    string targetAccountNumber,
    ChangeBalanceRequest req,
    IAccountRepository accountRepo) =>
{
    if (req.Amount <= 0)
        return Results.BadRequest(
            "Transfer amount must be greater than 0"
        );

    if (req.AccountNumber == targetAccountNumber)
        return Results.BadRequest(
            "Cannot transfer to same account"
        );

    var fromAccount = await accountRepo
        .GetAccount(req.AccountNumber);

    var toAccount = await accountRepo
        .GetAccount(targetAccountNumber);

    if (fromAccount == null || toAccount == null)
        return Results.NotFound("Account not found");

    if (fromAccount.CustomerId != req.CustomerId)
        return Results.BadRequest(
            "Source account does not belong to customer"
        );

    var newFromBalance =
        fromAccount.Balance - req.Amount;

    if (newFromBalance < 0)
        return Results.BadRequest("Not enough balance");

    var newToBalance =
        toAccount.Balance + req.Amount;

    await accountRepo.UpdateBalance(
        fromAccount.CustomerId,
        fromAccount.AccountNumber,
        newFromBalance
    );

    await accountRepo.UpdateBalance(
        toAccount.CustomerId,
        toAccount.AccountNumber,
        newToBalance
    );

    return Results.Ok(new
    {
        FromAccount = fromAccount.AccountNumber,
        OldFromBalance = fromAccount.Balance,
        NewFromBalance = newFromBalance,

        ToAccount = toAccount.AccountNumber,
        OldToBalance = toAccount.Balance,
        NewToBalance = newToBalance
    });
});

app.MapGet("/currency-rates", async (
    ICurrencyRateRepository repo) =>
{
    var rates = await repo.GetAll();

    return Results.Ok(rates);
});

app.MapPut("/currency-rates/{code}", async (
    string code,
    double rate,
    ICurrencyRateRepository repo,
    IHubContext<CurrencyRateHub> hub) =>
{
    code = code.ToUpper();

    await repo.Upsert(code, rate);

    var updatedRate = new CurrencyRate
    {
        Code = code,
        Rate = rate
    };

    await hub.Clients.All.SendAsync(
        "CurrencyRateUpdated",
        updatedRate
    );

    return Results.Ok(updatedRate);
});

app.Run();