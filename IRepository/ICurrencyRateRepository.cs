using Shared;

namespace IRepository;

public interface ICurrencyRateRepository
{
    Task<List<CurrencyRate>> GetAll();

    Task Upsert(string code, double rate);
}