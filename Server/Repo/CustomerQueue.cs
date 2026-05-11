using db;
using Microsoft.Data.Sqlite;

namespace Repository;

public class CustomerQueueRepos
{
    private readonly DB _db = new();

    public async Task Enqueue(int value)
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO CustomerQueue(number, status, created_date)
            VALUES ($1, $2, $3);
        """;

        command.Parameters.AddWithValue("$1", value);
        command.Parameters.AddWithValue("$2", "Waiting");
        command.Parameters.AddWithValue("$3", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int?> Dequeue()
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        using var selectCommand = connection.CreateCommand();
        selectCommand.Transaction = transaction;

        selectCommand.CommandText = """
            SELECT rowid, number
            FROM CustomerQueue
            WHERE status = 'Waiting'
            ORDER BY created_date ASC
            LIMIT 1;
        """;

        long rowId;
        int number;

        using (var reader = await selectCommand.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                transaction.Commit();
                return null;
            }

            rowId = reader.GetInt64(0);
            number = reader.GetInt32(1);
        }

        using var updateCommand = connection.CreateCommand();
        updateCommand.Transaction = transaction;

        updateCommand.CommandText = """
            UPDATE CustomerQueue
            SET status = 'Called'
            WHERE rowid = $rowId;
        """;

        updateCommand.Parameters.AddWithValue("$rowId", rowId);

        await updateCommand.ExecuteNonQueryAsync();

        transaction.Commit();

        return number;
    }

    public async Task<List<int>> GetActiveNumbers()
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT number
            FROM CustomerQueue
            WHERE status = 'Waiting' OR status = 'Called';
        """;

        using var reader = await command.ExecuteReaderAsync();

        var numbers = new List<int>();

        while (await reader.ReadAsync())
        {
            numbers.Add(reader.GetInt32(0));
        }

        return numbers;
    }

    public async Task CleanQueue()
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM CustomerQueue;
        """;

        await command.ExecuteNonQueryAsync();
    }
}