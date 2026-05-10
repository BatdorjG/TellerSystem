using System.Reflection;
using QMS;

namespace QMSTests;

public class QMSTests
{
    [Fact]
    public async Task GenerateTicketImageAsync_ReturnsImageBytes()
    {
        var handler = new FakeHttpMessageHandler("25");

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var service = new QueueTicketService(client);

        byte[]? result = await service.GenerateTicketImageAsync();

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GenerateTicketImageAsync_InvalidNumber_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler("INVALID");

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var service = new QueueTicketService(client);

        byte[]? result = await service.GenerateTicketImageAsync();

        Assert.Null(result);
    }
}
