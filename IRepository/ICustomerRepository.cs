using Shared;
namespace Repository;
public interface ICustomerRepository
{
    Task AddCustomer(
        string name,
        string registNumber
    );

    Task<List<Account>> GetAccountsByCustomerId(
        int customerId
    );

    Task<Customer?> GetCustomerByCustomerId(
        int customerId
    );

    Task<Customer?> GetCustomerByRegistNumber(
        string registNumber
    );

    Task<List<Customer>> GetAllCustomers();
}