using Accessor.Functions.Data;
using Accessor.Functions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Accessor.Functions.Services;

public class MessageProcessor
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly DatabaseService _dbService;

    public MessageProcessor(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
        _dbService = new DatabaseService(configuration, logger);
    }

    public async Task ProcessMessageAsync(string message, string invocationId, string? correlationId)
    {
        try
        {
            _logger.LogInformation("[Processor] Processing message: {message}", message);
            await _dbService.InsertMessageAsync(message, invocationId);

            if (!string.IsNullOrEmpty(correlationId))
            {
                await SendCallbackAsync(correlationId, message);
            }

            _logger.LogInformation("[Processor] Done. InvocationId: {InvocationId}", invocationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Processor] Failed. InvocationId: {InvocationId}", invocationId);
            throw;
        }
    }

    private async Task SendCallbackAsync(string correlationId, string originalMessage)
    {
        string connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        string callbackQueueName = "callbackqueue";

        var client = new ServiceBusClient(connectionString);
        var sender = client.CreateSender(callbackQueueName);

        var callback = new ServiceBusMessage($"Callback for: {originalMessage}")
        {
            CorrelationId = correlationId
        };

        await sender.SendMessageAsync(callback);
        _logger.LogInformation("[Processor] Sent callback message with CorrelationId: {CorrelationId}", correlationId);
    }
}