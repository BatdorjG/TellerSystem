using db;
using IRepository;
using Shared;

namespace Repository;

public class TellerRepository : ITellerRepository
{
    private readonly DB _db = new();

    public async Task AddTeller(string name, string password)
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO Teller(name, password)
            VALUES ($name, $password);
        """;

        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$password", password);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Teller?> GetTeller(string name)
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT id, name, password
            FROM Teller
            WHERE name = $name
            LIMIT 1;
        """;

        command.Parameters.AddWithValue("$name", name);

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new Teller(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2)
        );
    }

    public async Task<List<Teller>> GetAllTellers()
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT id, name, password
            FROM Teller;
        """;

        using var reader = await command.ExecuteReaderAsync();

        var tellers = new List<Teller>();

        while (await reader.ReadAsync())
        {
            tellers.Add(new Teller(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2)
            ));
        }

        return tellers;
    }
}
