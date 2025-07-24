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
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "send")] HttpRequestData req
        )
    {

        _logger.LogInformation("ProducerFunction triggered to send a message to Service Bus.");
        var response = req.CreateResponse();

        try
        {
            string messageBody = await req.ReadAsStringAsync();
            string connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            const string queueName = "myqueue";
            const string callbackQueueName = "callbackqueue";

            var client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(queueName);

            // Generate a unique correlation ID for this message
            string correlationId = Guid.NewGuid().ToString();
            var message = new ServiceBusMessage(messageBody);
            message.CorrelationId = correlationId;
            message.ReplyTo = callbackQueueName; // Optional: specify callback queue

            await sender.SendMessageAsync(message);
            _logger.LogInformation("Message sent to Service Bus with CorrelationId: {correlationId}", correlationId);




        // Wait for callback message in callbackQueue
            bool callbackReceived = await WaitForCallbackMessage(client, callbackQueueName, correlationId);

            if (callbackReceived)
            {
                response.StatusCode = HttpStatusCode.OK;
                await response.WriteStringAsync("Message sent and callback received successfully.");
            }
            else
            {
                response.StatusCode = HttpStatusCode.RequestTimeout;
                await response.WriteStringAsync("Message sent but callback not received within timeout.");
            }



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