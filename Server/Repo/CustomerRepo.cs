using db;

namespace Repository;

public class CustomerRepos
{
    private readonly DB _db = new();

    public async Task AddCustomer(string name, string registNumber)
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO Customer(name, registNumber)
            VALUES ($name, $registNumber);
        """;

        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$registNumber", registNumber);

        await command.ExecuteNonQueryAsync();
    }
    public async Task<List<Account>> GetAccountsByCustomerId(int customerId)
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                id,
                customer_id,
                account_number,
                account_type,
                balance,
                created_date
            FROM Account
            WHERE customer_id = $customerId;
        """;

        command.Parameters.AddWithValue("$customerId", customerId);

        using var reader = await command.ExecuteReaderAsync();

        var accounts = new List<Account>();

        while (await reader.ReadAsync())
        {
            accounts.Add(new Account(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetInt64(5)
            ));
        }

        return accounts;
    }

    public async Task<Customer?> GetCustomerByCustomerId(int customerId)
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                id,
                name,
                registNumber
            FROM Customer
            WHERE id = $customerId
            LIMIT 1;
        """;

        command.Parameters.AddWithValue("$customerId", customerId);

        using var reader = await command.ExecuteReaderAsync();
                
        if (!await reader.ReadAsync())
            return null;

        var customer = new Customer(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2)
        );

        return customer;
    }

    public async Task<Customer?> GetCustomerByRegistNumber(string registNumber)
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT id, name, registNumber
            FROM Customer
            WHERE registNumber = $registNumber
            LIMIT 1;
        """;

        command.Parameters.AddWithValue("$registNumber", registNumber);

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new Customer(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2)
        );
    }

    public async Task<List<Customer>> GetAllCustomers()
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT id, name, registNumber
            FROM Customer;
        """;

        using var reader = await command.ExecuteReaderAsync();

        var customers = new List<Customer>();

        while (await reader.ReadAsync())
        {
            customers.Add(new Customer(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2)
            ));
        }

        return customers;
    }
}

public record Customer(
    int Id,
    string Name,
    string RegistNumber
);