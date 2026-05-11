using db;

namespace Repository;

public record CurrencyRate
{
    public string Code { get; set; } = "";
    public double Rate { get; set; }
}

public class CurrencyRateRepos
{
    private readonly DB _db = new();

    public async Task<List<CurrencyRate>> GetAll()
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT code, rate
            FROM CurrencyRate
            ORDER BY code;
        """;

        using var reader = await command.ExecuteReaderAsync();

        var rates = new List<CurrencyRate>();

        while (await reader.ReadAsync())
        {
            rates.Add(new CurrencyRate
            {
                Code = reader.GetString(0),
                Rate = reader.GetDouble(1)
            });
        }

        return rates;
    }

    public async Task Upsert(string code, double rate)
    {
        using var connection = _db.Connect();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO CurrencyRate(code, rate)
            VALUES ($code, $rate)
            ON CONFLICT(code) DO UPDATE SET
                rate = excluded.rate;
        """;

        command.Parameters.AddWithValue("$code", code.ToUpper());
        command.Parameters.AddWithValue("$rate", rate);

        await command.ExecuteNonQueryAsync();
    }
}