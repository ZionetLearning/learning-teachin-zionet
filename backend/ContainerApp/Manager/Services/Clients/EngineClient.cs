using System.Text.Json;
using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using Manager.Models.QueueMessages;
using Manager.Models.Speech;

namespace Manager.Services.Clients;

public class EngineClient : IEngineClient
{
    private readonly ILogger<EngineClient> _logger;
    private readonly DaprClient _daprClient;

    public EngineClient(ILogger<EngineClient> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    public async Task<(bool success, string message)> ProcessTaskAsync(TaskModel task)
    {
        _logger.LogInformation(
            "Inside: {Method} in {Class}",
            nameof(ProcessTaskAsync),
            nameof(EngineClient)
        );

        try
        {
            var payload = JsonSerializer.SerializeToElement(task);
            var message = new Message
            {
                ActionName = MessageAction.CreateTask,
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

    public async Task<ChatResponseDto> ChatAsync(ChatRequestDto dto, CancellationToken ct = default)
    {
        _logger.LogInformation("Invoke Engine /chat synchronously (thread {Thread})", dto.ThreadId);

        return await _daprClient.InvokeMethodAsync<ChatRequestDto, ChatResponseDto>(
            appId: AppIds.Engine,
            methodName: "chat",
            data: dto,
            cancellationToken: ct);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with speech engine");
            return null;
        }
    }
}
