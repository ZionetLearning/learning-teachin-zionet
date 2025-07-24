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
        _logger.LogInformation("ProducerFunction triggered to send a message to Service Bus.");
        var response = req.CreateResponse();

        try
        {
            string messageBody = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(messageBody))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Message body cannot be empty.");
                return response;
            }

            string connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("ServiceBusConnectionString environment variable is not set.");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync("Service Bus connection string not configured.");
                return response;
            }

            const string queueName = "myqueue";
            const string callbackQueueName = "callbackqueue";

            await using var client = new ServiceBusClient(connectionString);
            await using var sender = client.CreateSender(queueName);

            // Generate a unique correlation ID for this message
            string correlationId = Guid.NewGuid().ToString();
            var message = new ServiceBusMessage(messageBody)
            {
                CorrelationId = correlationId,
                ReplyTo = callbackQueueName,
                TimeToLive = TimeSpan.FromMinutes(5) // Set TTL to prevent infinite retries
            };

            await sender.SendMessageAsync(message);
            _logger.LogInformation("Message sent to Service Bus with CorrelationId: {CorrelationId}", correlationId);

            // Wait for callback message in callbackQueue
            bool callbackReceived = await WaitForCallbackMessage(client, callbackQueueName, correlationId);

            if (callbackReceived)
            {
                response.StatusCode = HttpStatusCode.OK;
                await response.WriteStringAsync($"Message sent and callback received successfully. CorrelationId: {correlationId}");
            }
            else
            {
                response.StatusCode = HttpStatusCode.RequestTimeout;
                await response.WriteStringAsync($"Message sent but callback not received within timeout. CorrelationId: {correlationId}");
            }
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(ex, "Service Bus error: {Reason}", ex.Reason);
            response.StatusCode = HttpStatusCode.ServiceUnavailable;
            await response.WriteStringAsync("Service Bus is currently unavailable. Please try again later.");
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
        await using var receiver = client.CreateReceiver(callbackQueueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });

        try
        {
            // Set timeout for waiting (e.g., 30 seconds)
            var timeout = TimeSpan.FromSeconds(30);
            using var cancellationTokenSource = new CancellationTokenSource(timeout);
            var cancellationToken = cancellationTokenSource.Token;

            _logger.LogInformation("Waiting for callback message with CorrelationId: {CorrelationId}", correlationId);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Receive message with timeout
                    var receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), cancellationToken);
                    
                    if (receivedMessage != null)
                    {
                        _logger.LogInformation("Received callback message with CorrelationId: {ReceivedCorrelationId}", receivedMessage.CorrelationId);
                        
                        // Check if this is the callback for our message
                        if (receivedMessage.CorrelationId == correlationId)
                        {
                            _logger.LogInformation("Callback matched for CorrelationId: {CorrelationId}", correlationId);
                            
                            // Complete the callback message to remove it from the queue
                            await receiver.CompleteMessageAsync(receivedMessage);
                            return true;
                        }
                        else
                        {
                            // This callback is for a different message, abandon it so it can be processed again
                            _logger.LogWarning("Callback message CorrelationId mismatch. Expected: {Expected}, Received: {Received}", 
                                correlationId, receivedMessage.CorrelationId);
                            await receiver.AbandonMessageAsync(receivedMessage);
                        }
                    }
                    else
                    {
                        // No message available, wait a bit before trying again
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessageNotFound)
                {
                    // No message available, continue waiting
                    await Task.Delay(1000, cancellationToken);
                }
            }

            _logger.LogWarning("Timeout waiting for callback message with CorrelationId: {CorrelationId}", correlationId);
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Callback wait cancelled for CorrelationId: {CorrelationId}", correlationId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error waiting for callback message with CorrelationId: {CorrelationId}", correlationId);
            return false;
        }
    }
}