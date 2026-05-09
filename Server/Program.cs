using System.Net;
using System.Security.Cryptography.X509Certificates;
using QueueService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton<CustomerQueue>();

builder.Services.AddSingleton<SocketServer>();

builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<SocketServer>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/CustomerQueue", (CustomerQueue queue) =>
{
    var value = queue.EnqueueCustomer();
    return Results.Ok(value);
});

app.MapGet("/NextCustomer", async (
    byte tellerId,
    CustomerQueue queue,
    SocketServer socketServer) =>
{
    if (!queue.TryDequeueCustomer(out byte customerNumber))
    {
        return Results.NotFound("No customers waiting.");
    }

    byte displayId = tellerId;

    await socketServer.SendCallAsync(displayId, tellerId, customerNumber);

    return Results.Ok(new
    {
        tellerId,
        displayId,
        customerNumber
    });
});

app.Run();
