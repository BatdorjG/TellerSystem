using IRepository;
using Repository;
using QueueService;

namespace Services;

public static class ServiceCollection
{
    public static IServiceCollection AddServices(
        this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddSignalR();

        services.AddSingleton<ITellerRepository, TellerRepository>();
        services.AddSingleton<ICustomerRepository, CustomerRepository>();
        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<ICurrencyRateRepository, CurrencyRateRepository>();
        services.AddSingleton<ICustomerQueueRepository, CustomerQueueRepository>();
        services.AddSingleton<CustomerQueue>();
        services.AddSingleton<SocketServer>();

        services.AddHostedService(provider =>
            provider.GetRequiredService<SocketServer>());

        return services;
    }
}