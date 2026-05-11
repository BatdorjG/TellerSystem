using System.Data.Common;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using QueueService;
using Repository; 
using db;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton<CustomerQueue>();

builder.Services.AddSingleton<SocketServer>();
builder.Services.AddSingleton<CurrencyRateRepos>();
builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<SocketServer>());

var app = builder.Build();

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

// app.MapGet("/", async (CurrencyRateRepos currencyRateRepos) =>
// {
//     var rates = currencyRateRepos.GetRates();

//     return Results.Ok(rates);
// });

app.Run();
