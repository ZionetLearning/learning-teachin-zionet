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
    FunctionContext context)
    {
        _logger.LogInformation("ServiceBusConsumerFunction triggered to process a message from Service Bus.");
        _logger.LogInformation("Message received with CorrelationId: {correlationId} and message: {Message}", message.CorrelationId, message.Body.ToString());
        using var activity = new Activity("ProcessServiceBusMessage");
        activity.Start();

        await _processor.ProcessMessageAsync(message.Body.ToString(), context.InvocationId, message.CorrelationId);

        activity.Stop();
    }

}