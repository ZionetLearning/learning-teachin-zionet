using Accessor.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

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
        using var activity = new Activity("ProcessServiceBusMessage");
        activity.Start();

        string body = message.Body.ToString();
        string? correlationId = message.CorrelationId;

        await _processor.ProcessMessageAsync(body, context.InvocationId, correlationId);

        activity.Stop();
    }

}