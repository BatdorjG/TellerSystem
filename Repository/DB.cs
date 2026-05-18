using System.ComponentModel;
using Microsoft.Data.Sqlite;

namespace db;

public class DB
{
    public SqliteConnection Connect()
    {
        return new SqliteConnection("Data Source=Bank.db");
    }

    public void Init()
    {
        var connection = Connect();

        connection.Open();

        using var command = connection.CreateCommand();
        
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS CustomerQueue  (
                number INTEGER NOT NULL, 
                status TEXT NOT NULL,
                created_date INTEGER NOT NULL
            );

            CREATE INDEX idx_queue_status
            ON CustomerQueue(status);


            CREATE TABLE IF NOT EXISTS Customer  (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                registNumber TEXT NOT NULL UNIQUE
            );

            CREATE INDEX idx_customer_regNum
            ON Customer(registNumber);


            CREATE TABLE IF NOT EXISTS Teller  (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                password TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS CurrencyRate (
                code TEXT NOT NULL PRIMARY KEY,
                rate REAL NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Account (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                customer_id INTEGER NOT NULL,
                account_number TEXT NOT NULL UNIQUE,
                account_type TEXT NOT NULL,
                balance INTEGER NOT NULL DEFAULT 0,
                created_date INTEGER NOT NULL,

                FOREIGN KEY (customer_id) REFERENCES Customer(id)
            );

            CREATE INDEX IF NOT EXISTS idx_account_customer
            ON Account(customer_id);
        """;
        command.ExecuteNonQuery();
    }
}