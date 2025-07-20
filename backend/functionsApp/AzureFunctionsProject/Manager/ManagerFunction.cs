using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

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