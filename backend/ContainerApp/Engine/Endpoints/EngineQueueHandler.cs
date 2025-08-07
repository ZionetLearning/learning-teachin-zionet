using System.Text.Json;
using Engine.Messaging;
using Engine.Models;
using Engine.Services;

namespace Engine.Endpoints;

public class EngineQueueHandler : IQueueHandler<Message>
{
    private readonly IEngineService _engine;
    private readonly ILogger<EngineQueueHandler> _logger;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;
    public EngineQueueHandler(IEngineService engine, ILogger<EngineQueueHandler> logger)
    {
        _engine = engine;
        _logger = logger;
        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
        {
            [MessageAction.CreateTask] = HandleCreateTaskAsync,
            [MessageAction.TestLongTask] = HandleTestLongTaskAsync
        };
    }

    public async Task HandleAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        if (_handlers.TryGetValue(message.ActionName, out var handler))
        {
            await handler(message, renewLock, cancellationToken);
        }
        else
        {
            _logger.LogWarning("No handler for action {Action}", message.ActionName);
        }
    }
    private async Task HandleCreateTaskAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        var payload = message.Payload.Deserialize<TaskModel>();
        if (payload is null)
        {
            _logger.LogWarning("Invalid payload for CreateTask");
            return;
        }

        _logger.LogDebug("Processing task {Id}", payload.Id);
        await _engine.ProcessTaskAsync(payload, cancellationToken);
        _logger.LogInformation("Task {Id} processed", payload.Id);
    }

    private Task HandleTestLongTaskAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        _logger.LogInformation("TestLongTask received — not implemented yet.");
        return Task.CompletedTask;
    }
}

