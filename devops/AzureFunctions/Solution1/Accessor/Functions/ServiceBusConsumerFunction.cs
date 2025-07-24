using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Diagnostics;

namespace Accessor.Functions;

public class ServiceBusConsumerFunction
{
    private readonly ILogger<ServiceBusConsumerFunction> _logger;
    private readonly IConfiguration _configuration;

    public ServiceBusConsumerFunction(ILogger<ServiceBusConsumerFunction> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Function("ServiceBusConsumerFunction")]
    public async Task RunAsync(
        [ServiceBusTrigger("myqueue", Connection = "ServiceBusConnectionString")] string message,
        FunctionContext context)
    {
        using var activity = new Activity("ProcessServiceBusMessage");
        activity.Start();
        
        var invocationId = context.InvocationId;
        _logger.LogInformation("[Consumer] Processing message. InvocationId: {InvocationId}, Message: {Message}", 
            invocationId, message);

        try
        {
            // Use IConfiguration for better configuration management
            var connectionConfig = GetDatabaseConfiguration();
            
            if (!IsValidConfiguration(connectionConfig))
            {
                _logger.LogError("[Consumer] Invalid database configuration. InvocationId: {InvocationId}", invocationId);
                throw new InvalidOperationException("Invalid database configuration");
            }

            var connectionString = BuildConnectionString(connectionConfig);
            
            _logger.LogInformation("[Consumer] Attempting database connection. InvocationId: {InvocationId}", invocationId);

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            _logger.LogInformation("[Consumer] Database connection established. InvocationId: {InvocationId}", invocationId);

            // Ensure table exists
            await EnsureTableExistsAsync(connection, invocationId);

            // Insert message with transaction for better reliability
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var insertCmd = new NpgsqlCommand("INSERT INTO messages (body, invocation_id) VALUES (@body, @invocationId)", connection, transaction);
                insertCmd.Parameters.AddWithValue("@body", message);
                insertCmd.Parameters.AddWithValue("@invocationId", invocationId);
                
                var rowsAffected = await insertCmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("[Consumer] Message processed successfully. InvocationId: {InvocationId}, RowsAffected: {RowsAffected}", 
                    invocationId, rowsAffected);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (NpgsqlException npgsqlEx)
        {
            _logger.LogError(npgsqlEx, "[Consumer] Database error. InvocationId: {InvocationId}, ErrorCode: {ErrorCode}, SqlState: {SqlState}", 
                invocationId, npgsqlEx.ErrorCode, npgsqlEx.SqlState);
            
            // Re-throw for Service Bus retry mechanism
            throw;
        }
        catch (InvalidOperationException configEx)
        {
            _logger.LogError(configEx, "[Consumer] Configuration error. InvocationId: {InvocationId}", invocationId);
            
            // Don't re-throw configuration errors - message will go to dead letter queue
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Consumer] Unexpected error. InvocationId: {InvocationId}, ErrorType: {ErrorType}", 
                invocationId, ex.GetType().Name);
            
            // Re-throw for Service Bus retry mechanism
            throw;
        }
        finally
        {
            activity.Stop();
        }
    }

    private DatabaseConfiguration GetDatabaseConfiguration()
    {
        return new DatabaseConfiguration
        {
            Host = _configuration["Database__Host"],
            Database = _configuration["Database__Database"],
            Username = _configuration["Database__Username"],
            Password = _configuration["Database__Password"]
        };
    }

    private static bool IsValidConfiguration(DatabaseConfiguration config)
    {
        return !string.IsNullOrEmpty(config.Host) &&
               !string.IsNullOrEmpty(config.Database) &&
               !string.IsNullOrEmpty(config.Username) &&
               !string.IsNullOrEmpty(config.Password);
    }

    private static string BuildConnectionString(DatabaseConfiguration config)
    {
        return $"Host={config.Host};Database={config.Database};Username={config.Username};Password={config.Password};SSL Mode=Require;Trust Server Certificate=true;Command Timeout=30;";
    }

    private async Task EnsureTableExistsAsync(NpgsqlConnection connection, string invocationId)
    {
        var checkTableCmd = new NpgsqlCommand(
            "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'messages')", 
            connection);
        
        var tableExists = (bool)await checkTableCmd.ExecuteScalarAsync();

        if (!tableExists)
        {
            _logger.LogWarning("[Consumer] Creating messages table. InvocationId: {InvocationId}", invocationId);
            
            var createTableCmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS messages (
                    id SERIAL PRIMARY KEY,
                    body TEXT NOT NULL,
                    invocation_id TEXT,
                    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
                )", connection);
            
            await createTableCmd.ExecuteNonQueryAsync();
            _logger.LogInformation("[Consumer] Messages table created. InvocationId: {InvocationId}", invocationId);
        }
    }

    private record DatabaseConfiguration
    {
        public string Host { get; init; } = string.Empty;
        public string Database { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}