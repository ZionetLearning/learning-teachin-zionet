using Dapr.Client;
using Manager.Constants;
using Manager.Models;

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
            await _daprClient.InvokeBindingAsync(QueueNames.ManagerToEngine, "create", task);

            _logger.LogDebug(
                "Task {TaskId} sent to Engine via binding '{Binding}'",
                task.Id,
                QueueNames.ManagerToEngine
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

        // «engine» — это app-id, с которым запускается сервис Engine (dapr run --app-id engine …)
        return await _daprClient.InvokeMethodAsync<ChatRequestDto, ChatResponseDto>(
            appId: "engine",
            methodName: "chat",
            data: dto,
            cancellationToken: ct);
    }
}
