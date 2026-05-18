using Shared;

namespace IRepository;

public interface IAccountRepository
{
    Task AddAccount(
        int customerId,
        string accountType
    );

    Task<Account?> GetAccount(
        string accountNumber
    );

    Task<List<Account>> GetAccountsByCustomerId(
        int customerId
    );

    Task UpdateBalance(
        int customerId,
        string accountNumber,
        int balance
    );

    Task DeleteAccount(
        int customerId,
        string accountNumber
    );
}