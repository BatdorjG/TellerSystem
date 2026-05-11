using System.Data.Common;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using QueueService;
using Repository; 
using db;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<CurrencyRateRepos>();
builder.Services.AddSingleton<CustomerQueue>();

builder.Services.AddSingleton<SocketServer>();
builder.Services.AddSingleton<CurrencyRateRepos>();
builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<SocketServer>());

var app = builder.Build();

app.MapHub<CurrencyRateHub>("/currencyRateHub");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/StartDB", () =>
{
    DB _db = new();
    _db.Init();
    return Results.Ok();
});

app.MapGet("/CleanQueue", async (CustomerQueue queue) =>
{
    await queue.CleanQueue();
    return Results.Ok();
});

app.MapGet("/CustomerQueue", async (CustomerQueue queue) =>
{
    var value = await queue.EnqueueCustomer();
    return Results.Ok(value);
});

app.MapGet("/NextCustomer", async (
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
