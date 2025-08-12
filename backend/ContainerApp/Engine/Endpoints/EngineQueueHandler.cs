using System.Text.Json;
using Engine.Constants;
using Engine.Messaging;
using Engine.Models;
using Engine.Services;

namespace Engine.Endpoints;

public class EngineQueueHandler : IQueueHandler<Message>
{
    private readonly IEngineService _engine;
    private readonly ILogger<EngineQueueHandler> _logger;
    private readonly IServiceProvider _sp; // lazy resolution container
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;

    public EngineQueueHandler(
        IEngineService engine,
        IServiceProvider sp,
        ILogger<EngineQueueHandler> logger)
    {
        _engine = engine;
        _sp = sp;
        _logger = logger;

        _handlers = new()
        {
            [MessageAction.CreateTask] = HandleCreateTaskAsync,
            [MessageAction.TestLongTask] = HandleTestLongTaskAsync,
            [MessageAction.ProcessingQuestionAi] = HandleProcessingQuestionAiAsync
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

    private async Task HandleCreateTaskAsync(Message message, Func<Task> _, CancellationToken ct)
    {
        var payload = message.Payload.Deserialize<TaskModel>();
        if (payload is null)
        {
            _logger.LogWarning("Invalid payload for CreateTask");
            return;
        }

        _logger.LogDebug("Processing task {Id}", payload.Id);
        await _engine.ProcessTaskAsync(payload, ct);
        _logger.LogInformation("Task {Id} processed", payload.Id);
    }

    private async Task HandleTestLongTaskAsync(Message message, Func<Task> renewLock, CancellationToken ct)
    {
        using var renewalCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
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
            catch (OperationCanceledException) { /* normal on shutdown */ }
        }, renewalCts.Token);

        try
        {
            var payload = message.Payload.Deserialize<TaskModel>();
            if (payload is null)
            {
                _logger.LogWarning("Invalid payload for TestLongTask");
                return;
            }

            _logger.LogInformation("Inside handler");
            await Task.Delay(TimeSpan.FromSeconds(80), ct);
            await _engine.ProcessTaskAsync(payload, ct);
        }
        finally
        {
            await renewalCts.CancelAsync();
            await renewalTask;
        }
    }

    private async Task HandleProcessingQuestionAiAsync(Message message, Func<Task> _, CancellationToken ct)
    {
        var payload = message.Payload.Deserialize<AiRequestModel>();
        if (payload is null)
        {
            _logger.LogWarning("Invalid payload for ProcessingQuestionAi");
            return;
        }

        if (string.IsNullOrWhiteSpace(payload.ThreadId))
        {
            _logger.LogWarning("ThreadId is required.");
            return;
        }

        // Lazily resolve AI services only when needed
        var aiService = _sp.GetRequiredService<IChatAiService>();
        var publisher = _sp.GetRequiredService<IAiReplyPublisher>();

        _logger.LogInformation("Received AI question {Id} from manager", payload.Id);

        var response = await aiService.ProcessAsync(payload, ct);
        await publisher.SendReplyAsync(response, $"{QueueNames.ManagerCallbackQueue}-out", ct);

        _logger.LogInformation("AI question {Id} processed", payload.Id);
    }
}