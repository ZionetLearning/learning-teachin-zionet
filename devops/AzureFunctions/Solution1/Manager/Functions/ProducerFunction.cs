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

    private async Task<bool> WaitForCallbackMessage(ServiceBusClient client, string callbackQueueName, string correlationId)
    {
        ServiceBusReceiver receiver = null;
        try
        {
            receiver = client.CreateReceiver(callbackQueueName);
            
            // Set timeout for waiting (e.g., 30 seconds)
            var timeout = TimeSpan.FromSeconds(30);
            var cancellationToken = new CancellationTokenSource(timeout).Token;

            _logger.LogInformation("Waiting for callback message with CorrelationId: {correlationId}", correlationId);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Receive message with timeout
                    ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), cancellationToken);
                    
                    if (receivedMessage != null)
                    {
                        _logger.LogInformation("Received callback message with CorrelationId: {receivedCorrelationId}", receivedMessage.CorrelationId);
                        
                        // Check if this is the callback for our message
                        if (receivedMessage.CorrelationId == correlationId)
                        {
                            _logger.LogInformation("Callback matched for CorrelationId: {correlationId}", correlationId);
                            
                            // Complete the callback message to remove it from the queue
                            await receiver.CompleteMessageAsync(receivedMessage);
                            return true;
                        }
                        else
                        {
                            // This callback is for a different message, abandon it so it can be processed again
                            _logger.LogWarning("Callback message CorrelationId mismatch. Expected: {expected}, Received: {received}", 
                                correlationId, receivedMessage.CorrelationId);
                            await receiver.AbandonMessageAsync(receivedMessage);
                        }
                    }
                }
                catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessageNotFound)
                {
                    // No message available, continue waiting
                    await Task.Delay(1000, cancellationToken); // Wait 1 second before trying again
                }
            }

            _logger.LogWarning("Timeout waiting for callback message with CorrelationId: {correlationId}", correlationId);
            return false; // Timeout reached
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Callback wait cancelled for CorrelationId: {correlationId}", correlationId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error waiting for callback message with CorrelationId: {correlationId}", correlationId);
            return false;
        }
        finally
        {
            if (receiver != null)
            {
                await receiver.DisposeAsync();
            }
        }
    }
}