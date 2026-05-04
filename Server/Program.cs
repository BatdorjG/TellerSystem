using System.Net;
using QueueService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
IPAddress ipAddress = ipHostInfo.AddressList[0];
IPEndPoint iPEndPoint = new(ipAddress, 11_000);
var _customerQueue = CustomerQueue.GetInstance();

app.MapGet("/CustomerQueue", () =>
{
    var value = _customerQueue.EnqueueCustomer();
    return value;
});

app.MapGet("/NextCustomer", (int tellerId) =>
{
    var value = _customerQueue.DequeueCustomer();
    
});

app.Run();
