using db;

namespace Repository;

public class AccountRepos
{
    private readonly DB _db = new();

    public async Task AddAccount(
        int customerId,
        string accountNumber,
        string accountType
    )
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO Account(
                customer_id,
                account_number,
                account_type,
                balance,
                created_date
            )
            VALUES (
                $customerId,
                $accountNumber,
                $accountType,
                0,
                $createdDate
            );
        """;

        command.Parameters.AddWithValue("$customerId", customerId);
        command.Parameters.AddWithValue("$accountNumber", accountNumber);
        command.Parameters.AddWithValue("$accountType", accountType);
        command.Parameters.AddWithValue(
            "$createdDate",
            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Account?> GetAccount(
        int customerId,
        string accountNumber
    )
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
            WHERE customer_id = $customerId
            AND account_number = $accountNumber
            LIMIT 1;
        """;

        command.Parameters.AddWithValue("$customerId", customerId);
        command.Parameters.AddWithValue("$accountNumber", accountNumber);

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new Account(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetInt32(4),
            reader.GetInt64(5)
        );
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

    public async Task UpdateBalance(
        int customerId,
        string accountNumber,
        int balance
    )
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE Account
            SET balance = $balance
            WHERE customer_id = $customerId
            AND account_number = $accountNumber;
        """;

        command.Parameters.AddWithValue("$balance", balance);
        command.Parameters.AddWithValue("$customerId", customerId);
        command.Parameters.AddWithValue("$accountNumber", accountNumber);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAccount(
        int customerId,
        string accountNumber
    )
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM Account
            WHERE customer_id = $customerId
            AND account_number = $accountNumber;
        """;

        command.Parameters.AddWithValue("$customerId", customerId);
        command.Parameters.AddWithValue("$accountNumber", accountNumber);

        await command.ExecuteNonQueryAsync();
    }
}

public record Account(
    int Id,
    int CustomerId,
    string AccountNumber,
    string AccountType,
    int Balance,
    long CreatedDate
);