using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Npgsql;


namespace Accessor.Functions;

public class ServiceBusConsumerFunction
{
    private readonly ILogger<ServiceBusConsumerFunction> _logger;

    public ServiceBusConsumerFunction(ILogger<ServiceBusConsumerFunction> logger)
    {
        _logger = logger;
    }

    [Function("ServiceBusConsumerFunction")]
    public async Task RunAsync(
        [ServiceBusTrigger("myqueue", Connection = "ServiceBusConnectionString")] string message)
    {
        _logger.LogInformation($"[Consumer] Received message: {message}");

        try
        {
            // Build PostgreSQL connection string from environment variables
            var host = Environment.GetEnvironmentVariable("Database__Host");
            var database = Environment.GetEnvironmentVariable("Database__Database");
            var username = Environment.GetEnvironmentVariable("Database__Username");
            var password = Environment.GetEnvironmentVariable("Database__Password");

            var connectionString = $"Host={host};Database={database};Username={username};Password={password};Ssl Mode=Require";

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand("INSERT INTO messages (body) VALUES (@body)", connection);
            cmd.Parameters.AddWithValue("@body", message);
            await cmd.ExecuteNonQueryAsync();

            _logger.LogInformation("[Consumer] Message inserted into PostgreSQL.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Consumer] Error while processing message.");
        }
    }
}