using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Npgsql;

namespace LibraryAPI.Infrastructure.Database;

/// <summary>
/// ✅ SOLUZIONE: Implementazione concreta del factory per le connessioni database
/// Centralizza la logica di creazione delle connessioni e la rende facilmente testabile
/// </summary>
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly string _databaseType;
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _databaseType = _configuration["DatabaseSettings:Type"] ?? "SqlServer";
        _connectionString = _configuration.GetConnectionString(_databaseType) ?? 
            throw new InvalidOperationException($"Connection string '{_databaseType}' not found.");
    }

    public string DatabaseType => _databaseType;

    public IDbConnection CreateConnection()
    {
        IDbConnection connection = _databaseType switch
        {
            "SqlServer" => new SqlConnection(_connectionString),
            "PostgreSQL" => new NpgsqlConnection(_connectionString),
            "SQLite" => new SqliteConnection(_connectionString),
            _ => throw new InvalidOperationException($"Unsupported database type: {_databaseType}")
        };

        connection.Open();
        return connection;
    }
}