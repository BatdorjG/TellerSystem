using System.Data.Common;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using QueueService;
using Repository; 
using db;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<CurrencyRateRepos>();
builder.Services.AddSingleton<CustomerQueue>();
builder.Services.AddSingleton<CustomerRepos>();
builder.Services.AddSingleton<AccountRepos>();
builder.Services.AddSingleton<TellerRepos>();
builder.Services.AddSingleton<SocketServer>();
builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<SocketServer>());

var app = builder.Build();

app.MapHub<CurrencyRateHub>("/currencyRateHub");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/db/init", () =>
{
    DB _db = new();
    _db.Init();
    return Results.Ok();
});

app.MapGet("/customer/queue/clean", async (CustomerQueue queue) =>
{
    await queue.CleanQueue();
    return Results.Ok();
});

app.MapGet("/customer/queue", async (CustomerQueue queue) =>
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
    int? customerNumber = await queue.DequeueCustomer();

    if (customerNumber == null)
    {
        return Results.NotFound("No customers waiting.");
    }

    await socketServer.SendCallAsync(displayId, tellerId, customerNumber.Value);

    return Results.Ok(new
    {
        tellerId,
        displayId,
        customerNumber = customerNumber.Value
    });
});

app.MapPost("/teller/login", async (
    TellerLoginRequest req,
    TellerRepos tellerRepo) =>
{
    var teller = await tellerRepo.GetTeller(req.Name);

    if (teller == null)
        return Results.NotFound("No such teller");

    if (teller.Password != req.Password)
        return Results.Unauthorized();

    return Results.Ok(teller);
});

app.MapPost("/customer/add", async (
    AddCustomerRequest req,
    CustomerRepos customerRepo,
    AccountRepos accountRepo) =>
{
    await customerRepo.AddCustomer(req.Name, req.RegistNumber);

    var customer = await customerRepo.GetCustomerByRegistNumber(req.RegistNumber);
    
    return Results.Ok(customer);
});

app.MapPost("/customer/account/add", async (
    AddAccountRequest req,
    CustomerRepos customserRepo,
    AccountRepos accountRepo) =>
{
    var customer = await customserRepo.GetCustomerByCustomerId(req.CustomerId);
    if (customer == null)
        return Results.BadRequest("No such customer");
    
    await accountRepo.AddAccount(
        customer.Id,
        req.AccountType
    );

    return Results.Ok(customer);
});

app.MapGet("/customer/{registerNumber}", async (
    string registerNumber,
    CustomerRepos customerRepo,
    AccountRepos accountRepo) =>
{
    var customer = await customerRepo.GetCustomerByRegistNumber(registerNumber);

    if (customer == null)
        return Results.NotFound("Customer not found");

    var accounts = await accountRepo.GetAccountsByCustomerId(customer.Id);

    return Results.Ok(new CustomerInfoResponse(
        customer.Id,
        customer.Name,
        customer.RegistNumber,
        accounts
    ));
});

app.MapPost("/account/change-balance", async (
    ChangeBalanceRequest req,
    AccountRepos accountRepo) =>
{
    var account = await accountRepo.GetAccount(
        req.CustomerId,
        req.AccountNumber
    );

    if (account == null)
        return Results.NotFound("Account not found");

    var newBalance = account.Balance + req.Amount;

    if (newBalance < 0)
        return Results.BadRequest("Not enough balance");

    await accountRepo.UpdateBalance(
        req.CustomerId,
        req.AccountNumber,
        newBalance
    );

    return Results.Ok(new
    {
        account.AccountNumber,
        OldBalance = account.Balance,
        NewBalance = newBalance
    });
});

app.MapGet("/CurrencyRates", async (CurrencyRateRepos repo) =>
{
    var rates = await repo.GetAll();
    return Results.Ok(rates);
});

app.MapPut("/CurrencyRates/{code}", async (
    string code,
    double rate,
    CurrencyRateRepos repo,
    IHubContext<CurrencyRateHub> hub) =>
{
    code = code.ToUpper();

    await repo.Upsert(code, rate);

    var updatedRate = new CurrencyRate
    {
        Code = code,
        Rate = rate
    };

    await hub.Clients.All.SendAsync("CurrencyRateUpdated", updatedRate);

    return Results.Ok(updatedRate);
});

app.Run();

record TellerLoginRequest(
    string Name,
    string Password
);

record AddCustomerRequest(
    string Name,
    string RegistNumber,
    string AccountNumber,
    string AccountType
);

record AddAccountRequest(
    int CustomerId,
    string AccountNumber,
    string AccountType
);

record ChangeBalanceRequest(
    int CustomerId,
    string AccountNumber,
    int Amount
);

record CustomerInfoResponse(
    int Id,
    string Name,
    string RegistNumber,
    List<Account> Accounts
);