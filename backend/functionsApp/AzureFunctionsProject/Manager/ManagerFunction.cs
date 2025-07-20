using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsProject.Manager;

public class ManagerFunction
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ManagerFunction> _logger;

    public ManagerFunction(ServiceBusClient client, ILogger<ManagerFunction> logger)
    {
        _sender = client.CreateSender("incoming-queue");
        _logger = logger;
    }

}
