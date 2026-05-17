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