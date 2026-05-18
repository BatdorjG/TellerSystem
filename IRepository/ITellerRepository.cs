using Shared;

namespace IRepository;

public interface ITellerRepository
{
    Task AddTeller(
        string name,
        string password
    );

    Task<Teller?> GetTeller(
        string name
    );

    Task<List<Teller>> GetAllTellers();
}