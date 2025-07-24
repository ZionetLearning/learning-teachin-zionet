using Accessor.Functions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace Accessor.Functions.Data;

public class DatabaseService
{
	private readonly DatabaseConfiguration _config;
	private readonly ILogger _logger;

	public DatabaseService(IConfiguration configuration, ILogger logger)
	{
		_logger = logger;
		_config = new DatabaseConfiguration
		{
			Host = configuration["Database__Host"],
			Database = configuration["Database__Database"],
			Username = configuration["Database__Username"],
			Password = configuration["Database__Password"]
		};

		if (!IsValidConfiguration())
			throw new InvalidOperationException("Invalid database configuration.");
	}

	private bool IsValidConfiguration()
	{
		return !string.IsNullOrEmpty(_config.Host) &&
			   !string.IsNullOrEmpty(_config.Database) &&
			   !string.IsNullOrEmpty(_config.Username) &&
			   !string.IsNullOrEmpty(_config.Password);
	}

	private string BuildConnectionString()
	{
		return $"Host={_config.Host};Database={_config.Database};Username={_config.Username};Password={_config.Password};SSL Mode=Require;Trust Server Certificate=true;";
	}

	public async Task InsertMessageAsync(string message, string invocationId)
	{
		await using var connection = new NpgsqlConnection(BuildConnectionString());
		await connection.OpenAsync();

		await EnsureTableExistsAsync(connection, invocationId);

		await using var transaction = await connection.BeginTransactionAsync();
		try
		{
			var cmd = new NpgsqlCommand("INSERT INTO messages (body, invocation_id) VALUES (@body, @invocationId)", connection, transaction);
			cmd.Parameters.AddWithValue("@body", message);
			cmd.Parameters.AddWithValue("@invocationId", invocationId);
			await cmd.ExecuteNonQueryAsync();

			await transaction.CommitAsync();
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
	}

	private async Task EnsureTableExistsAsync(NpgsqlConnection connection, string invocationId)
	{
		var checkTableCmd = new NpgsqlCommand("SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'messages')", connection);
		var tableExists = (bool)await checkTableCmd.ExecuteScalarAsync();

		if (!tableExists)
		{
			_logger.LogWarning("[Database] Creating table. InvocationId: {InvocationId}", invocationId);
			var createCmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS messages (
                    id SERIAL PRIMARY KEY,
                    body TEXT NOT NULL,
                    invocation_id TEXT,
                    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                )", connection);

			await createCmd.ExecuteNonQueryAsync();
		}
	}
}