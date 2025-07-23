using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RabbitMQ.Client;
using System.Text;
using AccessorService.Models;
using System.Net.Http.Json;

namespace Accessor.Functions;

public class RabbitConsumerFunction
{
    private readonly ILogger _logger;
    private readonly IMongoCollection<MessageDocument> _collection;

    public RabbitConsumerFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RabbitConsumerFunction>();

        var mongoUri = Environment.GetEnvironmentVariable("MongoConnection");
        var client = new MongoClient(mongoUri);
        var database = client.GetDatabase("MyDatabase");
        _collection = database.GetCollection<MessageDocument>("Messages");
    }

    [Function("RabbitConsumerFunction")]
    public async Task RunAsync([TimerTrigger("*/10 * * * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation($"[Consumer] Triggered at: {DateTime.UtcNow}");

        try
        {
            var rabbitUri = Environment.GetEnvironmentVariable("RabbitMQConnection");
            var callbackUrl = Environment.GetEnvironmentVariable("CallbackUrl");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(rabbitUri)
            };


            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var result = await channel.BasicGetAsync("myqueue", autoAck: true);

            if (result != null)
            {
                var message = Encoding.UTF8.GetString(result.Body.ToArray());
                _logger.LogInformation($"[Consumer] Received: {message}");

                var doc = new MessageDocument { Body = message };
                await _collection.InsertOneAsync(doc);
                _logger.LogInformation("[Consumer] Saved to MongoDB.");

                // Send confirmation to Manager
                using var httpClient = new HttpClient();
                var payload = new { status = "success", message = message };

                var response = await httpClient.PostAsJsonAsync(callbackUrl, payload);

                if (response.IsSuccessStatusCode)
                    _logger.LogInformation("[Consumer] Callback sent successfully.");
                else
                    _logger.LogError($"[Consumer] Failed to send callback. Status: {response.StatusCode}");
            }
            else
            {
                _logger.LogInformation("[Consumer] No message found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Consumer] Error: {ex.Message}");
        }
    }
}