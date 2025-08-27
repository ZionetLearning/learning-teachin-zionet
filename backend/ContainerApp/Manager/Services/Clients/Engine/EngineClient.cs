using System.Text.Json;
using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using Manager.Models.QueueMessages;
using Manager.Models.Speech;
using Manager.Services.Clients.Engine.Models;

namespace Manager.Services.Clients.Engine;

public class EngineClient : IEngineClient
{
    private readonly ILogger<EngineClient> _logger;
    private readonly DaprClient _daprClient;

    public EngineClient(ILogger<EngineClient> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    public async Task<(bool success, string message)> ProcessTaskLongAsync(TaskModel task)
    {
        _logger.LogInformation(
            "Inside: {Method} in {Class}",
            nameof(ProcessTaskLongAsync),
            nameof(EngineClient)
        );
        try
        {
            var payload = JsonSerializer.SerializeToElement(task);
            var message = new Message
            {
                ActionName = MessageAction.TestLongTask,
                Payload = payload
            };
            await _daprClient.InvokeBindingAsync($"{QueueNames.EngineQueue}-out", "create", message);

            _logger.LogDebug(
                "Task {TaskId} sent to Engine via binding '{Binding}'",
                task.Id,
                QueueNames.EngineQueue
            );
            return (true, "sent to engine");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task {TaskId} to Engine", task.Id);
            throw;
        }
    }

    public async Task<(bool success, string message)> ChatAsync(EngineChatRequest request)
    {
        _logger.LogInformation("Invoke Engine /chat synchronously (thread {Thread})", request.ThreadId);
        try
        {
            var requestMetadata = new UserContextMetadata
            {
                UserId = request.UserId.ToString()
            };

            var message = new Message
            {
                ActionName = MessageAction.ProcessingChatMessage,
                Payload = JsonSerializer.SerializeToElement(request),
                Metadata = JsonSerializer.SerializeToElement(requestMetadata)
            };

            var queueMetadata = new Dictionary<string, string>
            {
                ["sessionId"] = request.ThreadId.ToString()
            };

            await _daprClient.InvokeBindingAsync($"{QueueNames.EngineQueue}-out", "create", message, queueMetadata);

            _logger.LogDebug(
                "ProcessingChatMessage request for thread {ThreadId} sent to Engine via binding '{Binding}'",
                request.ThreadId,
                QueueNames.EngineQueue
            );
            return (true, "sent to engine");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send chat request to Engine");
            return (false, "failed to send chat request");
        }
    }

    public async Task<SpeechEngineResponse?> SynthesizeAsync(SpeechRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Forwarding speech synthesis request to engine");

            var result = await _daprClient.InvokeMethodAsync<SpeechRequest, SpeechEngineResponse>(
                appId: AppIds.Engine,
                methodName: "speech/synthesize",
                data: request,
                cancellationToken: cancellationToken);

            return result;
        }
        catch (InvocationException ex) when (ex.InnerException is HttpRequestException httpEx && httpEx.Message.Contains("408"))
        {
            _logger.LogWarning("Speech synthesis request timed out (408)");
            throw new TimeoutException("Speech synthesis request timed out", ex);
        }
        catch (InvocationException ex) when (ex.InnerException is HttpRequestException httpEx && httpEx.Message.Contains("499"))
        {
            _logger.LogWarning("Speech synthesis request was canceled by client (499)");
            throw new OperationCanceledException("Speech synthesis request was canceled by client", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with speech engine");
            return null;
        }
    }
}
