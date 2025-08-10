using Engine.Messaging;
using Engine.Models;
using Engine.Services;
using System.Text.Json;

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
        try
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
        catch (Exception)
        {
            _logger.LogError("Error while {Action}", message.ActionName);
            throw;
        }
    }
    private async Task HandleTestLongTaskAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        using var renewalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var renewalTask = Task.Run(async () =>
        {
            try
            {
                while (!renewalCts.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(20), renewalCts.Token);
                    await renewLock();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }, renewalCts.Token);
        try
        {
            var payload = message.Payload.Deserialize<TaskModel>();
            if (payload is null)
            {
                _logger.LogWarning("Invalid payload for CreateTask");
                return;
            }

            _logger.LogInformation("Inside handler");

            await Task.Delay(TimeSpan.FromSeconds(80), cancellationToken);
            await _engine.ProcessTaskAsync(payload, cancellationToken);
        }
        catch
        {
            _logger.LogError("Error while {Action}", message.ActionName);
            throw;
        }
        finally
        {
            await renewalCts.CancelAsync();
            await renewalTask;
        }
    }
}

