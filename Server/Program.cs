using QueueService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var _customerQueue = CustomerQueue.GetInstance();

app.MapGet("/CustomerQueue", () =>
{
    var value = _customerQueue.EnqueueCustomer();
    return value;
});

app.Run();
