using System.Text.Json;
using Engine.Constants;

//using Dapr.Client.Autogen.Grpc.v1;
using Engine.Messaging;
using Engine.Models;
using Engine.Services;

namespace Engine.Endpoints;

public class EngineQueueHandler : IQueueHandler<Message>
{
    private readonly IEngineService _engine;
    private readonly ILogger<EngineQueueHandler> _logger;
    private readonly Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>> _handlers;
    private readonly IChatAiService _aiService;
    private readonly IAiReplyPublisher _publisher;

    public EngineQueueHandler(IEngineService engine,
        IChatAiService aiService,
        IAiReplyPublisher publisher,
        ILogger<EngineQueueHandler> logger)
    {
        _engine = engine;
        _aiService = aiService;
        _publisher = publisher;
        _logger = logger;
        _handlers = new Dictionary<MessageAction, Func<Message, Func<Task>, CancellationToken, Task>>
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

    private async Task HandleTestLongTaskAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        var payload = message.Payload.Deserialize<TaskModel>();
        if (payload is null)
        {
            _logger.LogWarning("Invalid payload for CreateTask");
            return;
        }

        using var renewalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var renewalTask = Task.Run(async () =>
        {
            try
            {
                while (!renewalCts.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(20), renewalCts.Token);
                    _logger.LogDebug("Renewing lock at {Now}", DateTime.UtcNow);
                    await renewLock();
                    _logger.LogDebug("Lock renewed at {Now}", DateTime.UtcNow);
                }
            }
            catch (OperationCanceledException)
            {
                // expected
            }
        }, renewalCts.Token);

        try
        {
            _logger.LogInformation("Inside handler");

            await Task.Delay(TimeSpan.FromSeconds(80), cancellationToken);
            await _engine.ProcessTaskAsync(payload, cancellationToken);
        }
        finally
        {
            await renewalCts.CancelAsync();
            await renewalTask;
        }
        //That is simple example of how to use renewLock to prevent exciding message lock time 
        //_logger.LogInformation("Inside handler");
        //await renewLock();
        //await Task.Delay(TimeSpan.FromSeconds(50), cancellationToken);
        //_logger.LogInformation("After first pause");
        //await renewLock();
        //await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        //_logger.LogInformation("Processing task {Id}", payload.Id);
        //await _engine.ProcessTaskAsync(payload, cancellationToken);
        //_logger.LogInformation("Task {Id} processed", payload.Id);
    }

    private async Task HandleProcessingQuestionAiAsync(Message message, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        var payload = message.Payload.Deserialize<AiRequestModel>();
        if (payload is null)
        {
            _logger.LogWarning("Invalid payload for CreateTask");
            return;
        }

        _logger.LogInformation("Received AI question {Id} from manager", payload.Id);

        try
        {
            if (string.IsNullOrWhiteSpace(payload.ThreadId))
            {
                _logger.LogWarning("ThreadId is required.");
                return;
            }

            var response = await _aiService.ProcessAsync(payload, cancellationToken);

            await _publisher.SendReplyAsync(response, $"{QueueNames.ManagerCallbackQueue}-out", cancellationToken);

            _logger.LogInformation("AI question {Id} processed", payload.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process AI question {Id}", payload.Id);
            throw;
        }
    }
}
