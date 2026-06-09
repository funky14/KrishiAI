using System.Data;
using Microsoft.Data.SqlClient;

namespace KrishiAI.App.Services;

/// <summary>
/// Service for managing SQL Server database connections
/// Connects directly to Azure SQL Server database specified in connection string
/// </summary>
public class SqlServerConnectionService : ISqlServerConnectionService
{
    private readonly string _connectionString;
    private const string DefaultConnectionString = "Data Source=azuredemodb.database.windows.net;Initial Catalog=free-sql-db-4227077;Persist Security Info=True;User ID=sqladmin;Password=Amazon@810649;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";

    public SqlServerConnectionService()
    {
        // Use connection string from environment or use default
        _connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING") ?? DefaultConnectionString;
    }

    public async Task<SqlConnection> GetConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to connect to SQL Server database", ex);
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(string query, Dictionary<string, object>? parameters = null)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new SqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            var result = await command.ExecuteScalarAsync();
            return (T?)result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"SQL query failed: {query}", ex);
        }
    }

    public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new SqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            return await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"SQL query failed: {query}", ex);
        }
    }

    public async Task<SqlDataReader?> ExecuteReaderAsync(string query, Dictionary<string, object>? parameters = null)
    {
        try
        {
            var connection = await GetConnectionAsync();
            var command = new SqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            return await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"SQL query failed: {query}", ex);
        }
    }
}

/// <summary>
/// Interface for SQL Server connection service
/// </summary>
public interface ISqlServerConnectionService
{
    Task<SqlConnection> GetConnectionAsync();
    Task<T?> ExecuteScalarAsync<T>(string query, Dictionary<string, object>? parameters = null);
    Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null);
    Task<SqlDataReader?> ExecuteReaderAsync(string query, Dictionary<string, object>? parameters = null);
}
