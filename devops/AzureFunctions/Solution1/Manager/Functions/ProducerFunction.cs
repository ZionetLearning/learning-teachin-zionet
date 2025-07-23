using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using System.Net;

namespace Manager.Functions;

public class ProducerFunction
{
    private readonly ILogger<ProducerFunction> _logger;

    public ProducerFunction(ILogger<ProducerFunction> logger)
    {
        _logger = logger;
    }

    [Function("ProducerFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "send")] HttpRequestData req)
    {
        var response = req.CreateResponse();

        try
        {
            string messageBody = await req.ReadAsStringAsync();
            string connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            string queueName = "myqueue";

            var client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(queueName);

            await sender.SendMessageAsync(new ServiceBusMessage(messageBody));

            _logger.LogInformation("Message sent to Service Bus: {message}", messageBody);

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteStringAsync("Message sent to Service Bus.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to Service Bus.");

            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync("Failed to send message to Service Bus.");
        }

        return response;
    }
}