using Accessor.Functions.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Accessor.Functions.Functions;

public class ServiceBusConsumerFunction
{
    private readonly ILogger<ServiceBusConsumerFunction> _logger;
    private readonly MessageProcessor _processor;

    public ServiceBusConsumerFunction(
        ILogger<ServiceBusConsumerFunction> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _processor = new MessageProcessor(configuration, logger);
    }

    [Function("ServiceBusConsumerFunction")]
    public async Task RunAsync(
        [ServiceBusTrigger("myqueue", Connection = "ServiceBusConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        FunctionContext context)
    {
        using var activity = new Activity("ProcessServiceBusMessage");
        activity.Start();

        try
        {
            _logger.LogInformation("ServiceBusConsumerFunction triggered. MessageId: {MessageId}, CorrelationId: {CorrelationId}, DeliveryCount: {DeliveryCount}", 
                message.MessageId, message.CorrelationId, message.DeliveryCount);

            // Process the message
            await _processor.ProcessMessageAsync(message.Body.ToString(), context.InvocationId, message.CorrelationId);

            // If we reach here, processing was successful
            await messageActions.CompleteMessageAsync(message);
            _logger.LogInformation("Message processed successfully. MessageId: {MessageId}, CorrelationId: {CorrelationId}", 
                message.MessageId, message.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message. MessageId: {MessageId}, CorrelationId: {CorrelationId}, DeliveryCount: {DeliveryCount}", 
                message.MessageId, message.CorrelationId, message.DeliveryCount);

            // Check if we should retry or dead letter the message
            if (message.DeliveryCount >= 3) // Max retry attempts
            {
                _logger.LogWarning("Max delivery attempts reached. Dead lettering message. MessageId: {MessageId}", message.MessageId);
                await messageActions.DeadLetterMessageAsync(message, new Dictionary<string, object>
                {
                    ["DeadLetterReason"] = "MaxDeliveryCountExceeded",
                    ["DeadLetterErrorDescription"] = ex.Message
                });
            }
            else
            {
                _logger.LogInformation("Abandoning message for retry. MessageId: {MessageId}, DeliveryCount: {DeliveryCount}", 
                    message.MessageId, message.DeliveryCount);
                await messageActions.AbandonMessageAsync(message);
            }
        }
        finally
        {
            activity.Stop();
        }
    }
}