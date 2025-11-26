using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Dapr.Client;
using DotQueue;
using Engine.Helpers;
using Engine.Models;
using Engine.Models.Chat;
using Engine.Models.QueueMessages;
using Engine.Models.Sentences;
using Engine.Models.Words;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Engine.Services.Clients.AccessorClient.Models;

namespace Engine.Endpoints;

public class EngineQueueHandler : RoutedQueueHandler<Message, MessageAction>
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<EngineQueueHandler> _logger;
    private readonly ILogger<StreamingChatAIBatcher> _batcherLogger;
    private readonly IChatAiService _aiService;
    private readonly ISentencesService _sentencesService;
    private readonly IAiReplyPublisher _publisher;
    private readonly IAccessorClient _accessorClient;
    private readonly IChatTitleService _chatTitleService;
    private readonly IWordExplainService _wordExplainService;
    protected override MessageAction GetAction(Message message) => message.ActionName;
    protected override void Configure(RouteBuilder routes) => routes
        .On(MessageAction.CreateTask, HandleCreateTaskAsync)
        .On(MessageAction.TestLongTask, HandleTestLongTaskAsync)
        .On(MessageAction.ProcessingChatMessage, HandleProcessingChatMessageAsync)
        .On(MessageAction.ProcessingGlobalChatMessage, HandleProcessingChatMessageAsync)
        .On(MessageAction.ProcessingExplainMistake, HandleProcessingChatMessageAsync)
        .On(MessageAction.GenerateSentences, HandleSentenceGenerationAsync)
        .On(MessageAction.GenerateSplitSentences, HandleSentenceGenerationAsync)
        .On(MessageAction.GenerateWordExplain, HandleWordExplainAsync);
    public EngineQueueHandler(
        DaprClient daprClient,
        ILogger<EngineQueueHandler> logger,
        ILogger<StreamingChatAIBatcher> batcherLogger,
        IChatAiService aiService,
        IAiReplyPublisher publisher,
        IAccessorClient accessorClient,
        ISentencesService sentencesService,
        IChatTitleService chatTitleService,
        IWordExplainService wordExplainService) : base(logger)
    {
        _daprClient = daprClient;
        _logger = logger;
        _batcherLogger = batcherLogger;
        _aiService = aiService;
        _publisher = publisher;
        _accessorClient = accessorClient;
        _chatTitleService = chatTitleService;
        _sentencesService = sentencesService;
        _wordExplainService = wordExplainService;
    }
    private async Task HandleCreateTaskAsync(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var payload = PayloadValidation.DeserializeOrThrow<TaskModel>(message, _logger);
            PayloadValidation.ValidateTask(payload, _logger);
            using var _ = _logger.BeginScope("Processing TaskId: {TaskId}", payload.Id);
            _logger.LogInformation("Inside {Method}", nameof(HandleCreateTaskAsync));
            cancellationToken.ThrowIfCancellationRequested();
            if (payload is null)
            {
                _logger.LogWarning("Attempted to process a null task");
                throw new ArgumentNullException(nameof(message), "Task payload cannot be null");
            }

            _logger.LogInformation("Logged task: {Name}", payload.Name);
            await _daprClient.InvokeMethodAsync(HttpMethod.Post, "accessor", "tasks-accessor/task", payload, cancellationToken);
            _logger.LogInformation("Task {Id} forwarded to the Accessor service", payload.Id);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Transient error while processing task for action {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing task.", ex);
        }
    }

    private async Task HandleTestLongTaskAsync(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken cancellationToken)
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
                _logger.LogError("Renew Lock loop error");
                throw;
            }
        }, renewalCts.Token);
        try
        {
            var payload = PayloadValidation.DeserializeOrThrow<TaskModel>(message, _logger);
            PayloadValidation.ValidateTask(payload, _logger);
            await Task.Delay(TimeSpan.FromSeconds(80), cancellationToken);
            using var _ = _logger.BeginScope("Starting long task handler for Task {Id}", payload.Id);
            _logger.LogInformation("Inside {Method}", nameof(HandleTestLongTaskAsync));
            cancellationToken.ThrowIfCancellationRequested();
            if (payload is null)
            {
                _logger.LogWarning("Attempted to process a null task");
                throw new ArgumentNullException(nameof(message), "Task payload cannot be null");
            }

            _logger.LogInformation("Logged task: {Name}", payload.Name);
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Post, "accessor", "tasks-accessor/task", payload, cancellationToken);
            _logger.LogInformation("Task {Id} forwarded to the Accessor service", payload.Id);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Transient error while processing long task for action {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing long task.", ex);
        }
        finally
        {
            await renewalCts.CancelAsync();
            await renewalTask;
        }
    }

    private async Task HandleProcessingChatMessageAsync(
        Message message,
        IReadOnlyDictionary<string, string>? metadata,
        Func<Task> renewLock,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var seq = 0;
        Func<int> NextSeq = () => Interlocked.Increment(ref seq) - 1;

        EngineChatRequest? request = null;
        UserContextMetadata? userContext = null;
        CancellationTokenSource? renewalCts = null;
        Task? renewTask = null;
        Stopwatch? elapsed = null;

        try
        {
            (request, userContext) = DeserializeAndValidateChatRequest(message);
            using var _ = _logger.BeginScope(new { request.RequestId, request.ThreadId, request.UserId });

            renewalCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            renewTask = Task.Run(async () =>
            {
                try
                {
                    while (!renewalCts.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(20), renewalCts.Token);
                        await renewLock();
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Renew lock loop failed during chat streaming");
                }
            }, renewalCts.Token);

            elapsed = Stopwatch.StartNew();

            string? finalThreadJson = null;
            var total = new StringBuilder();

            var chatName = "Chat"; // todo: turn on title service

            EngineChatStreamResponse BuildResponse(
                ChatStreamStage stage,
                string? delta = null,
                string? toolCall = null,
                string? toolResult = null)
            {
                return new EngineChatStreamResponse
                {
                    RequestId = request.RequestId,
                    ThreadId = request.ThreadId,
                    UserId = request.UserId,
                    ChatName = chatName,
                    Stage = stage,
                    Delta = delta,
                    ToolCall = toolCall,
                    ToolResult = toolResult,
                    IsFinal = false,
                    ElapsedMs = elapsed.ElapsedMilliseconds
                };
            }

            await using var batcher = new StreamingChatAIBatcher(
                minChars: 80,
                maxLatency: TimeSpan.FromMilliseconds(250),
                makeChunk: batchedText => BuildResponse(ChatStreamStage.Model, delta: batchedText),
                sendAsync: async chunk =>
                {
                    chunk.Sequence = NextSeq();
                    if (chunk.Stage == ChatStreamStage.Model && !string.IsNullOrEmpty(chunk.Delta))
                    {
                        total.Append(chunk.Delta);

                    }

                    await _publisher.SendStreamAsync(userContext!, chunk, ct);
                },
                makeToolChunk: upd => BuildResponse(ChatStreamStage.Tool, toolCall: upd.ToolCall),
                makeToolResultChunk: upd => BuildResponse(ChatStreamStage.ToolResult, toolResult: upd.ToolResult),
                logger: _batcherLogger,
                ct: ct);

            await foreach (var upd in _aiService.ChatStreamAsync(request, ct))
            {
                await batcher.HandleUpdateAsync(upd);

                if (upd.IsFinal)
                {
                    finalThreadJson = upd.ThreadStoreJson;
                    break;
                }
            }

            await batcher.FlushAsync();

            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(finalThreadJson) ? "null" : finalThreadJson);
            var afHistory = doc.RootElement.Clone();

            await _accessorClient.UpsertHistorySnapshotAsync(new UpsertHistoryRequest
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Name = chatName,
                ChatType = request.ChatType.ToString().ToLowerInvariant(),
                History = afHistory
            }, ct);

            var finalChunk = new EngineChatStreamResponse
            {
                RequestId = request.RequestId,
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                ChatName = chatName,
                Sequence = NextSeq(),
                Delta = total.ToString(),
                Stage = ChatStreamStage.Final,
                IsFinal = true,
                ElapsedMs = elapsed.ElapsedMilliseconds
            };

            await _publisher.SendStreamAsync(userContext!, finalChunk, ct);

            _logger.LogInformation("Chat request {RequestId} processed successfully", request.RequestId);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            var canceled = new EngineChatStreamResponse
            {
                RequestId = request?.RequestId ?? string.Empty,
                ThreadId = request?.ThreadId ?? Guid.Empty,
                UserId = request?.UserId ?? Guid.Empty,
                ChatName = "Chat",
                Sequence = NextSeq(),
                Stage = ChatStreamStage.Canceled,
                IsFinal = true,
                ElapsedMs = elapsed?.ElapsedMilliseconds ?? 0
            };
            _logger.LogWarning("Operation cancelled while processing {Action}", message.ActionName);
            await _publisher.SendStreamAsync(
                MetadataValidation.DeserializeOrThrow<UserContextMetadata>(message, _logger),
                canceled,
                CancellationToken.None);
            throw;
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (Exception ex)
        {
            if (ct.IsCancellationRequested)
            {
                throw new OperationCanceledException("Operation was cancelled.", ex, ct);
            }

            _logger.LogError(ex, "Transient error while processing AI chat {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing AI chat.", ex);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("Chat request finished in {ElapsedMs} ms", sw.ElapsedMilliseconds);

            if (renewalCts is not null)
            {
                try
                {
                    await renewalCts.CancelAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error while cancelling renewal");
                }

                try
                {
                    if (renewTask is not null)
                    {
                        await renewTask;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error while awaiting renew lock task");
                }

                renewalCts.Dispose();
            }
        }
    }

    private (EngineChatRequest Request, UserContextMetadata UserContext)
    DeserializeAndValidateChatRequest(Message message)
    {
        var request = PayloadValidation.DeserializeOrThrow<EngineChatRequest>(message, _logger);
        PayloadValidation.ValidateEngineChatRequest(request, _logger);

        var userContext = MetadataValidation.DeserializeOrThrow<UserContextMetadata>(message, _logger);
        MetadataValidation.ValidateUserContext(userContext, _logger);

        if (request.UserId == Guid.Empty)
        {
            throw new NonRetryableException("UserId is required.");
        }

        if (request.TtlSeconds <= 0)
        {
            throw new NonRetryableException("TtlSeconds must be greater than 0.");
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now > request.SentAt + request.TtlSeconds)
        {
            _logger.LogWarning("Chat request {RequestId} expired. Skipping.", request.RequestId);
            throw new NonRetryableException("Request TTL expired.");
        }

        if (request.ChatType == ChatType.ExplainMistake)
        {
            if (request.AttemptId is null || request.GameType is null)
            {
                throw new NonRetryableException("AttemptId and GameType are required for ExplainMistake.");

            }
        }

        return (request, userContext);
    }

    private async Task HandleSentenceGenerationAsync(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var payload = PayloadValidation.DeserializeOrThrow<SentenceRequest>(message, _logger);

            PayloadValidation.ValidateSentenceGenerationRequest(payload, _logger);

            _logger.LogDebug("Processing sentence generation");
            // inject user interests
            var userInterests = await _accessorClient.GetUserInterestsAsync(payload.UserId, cancellationToken);
            if (userInterests == null)
            {
                _logger.LogWarning("No interests found for user {UserId}", payload.UserId);
                throw new NonRetryableException("User interests is null.");
            }

            var response = await _sentencesService.GenerateAsync(payload, userInterests, cancellationToken);
            var userId = payload.UserId;

            var sentencesResponse = new SentencesResponse
            {
                RequestId = payload.RequestId,
                Sentences = response.Sentences
            };
            await _publisher.SendGeneratedMessagesAsync(userId.ToString(), sentencesResponse, message.ActionName, cancellationToken);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Transient error while processing for action {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing.", ex);
        }
    }
    private async Task HandleWordExplainAsync(
    Message message,
    IReadOnlyDictionary<string, string>? metadata,
    Func<Task> renewLock,
    CancellationToken cancellationToken)
    {
        try
        {
            var payload = PayloadValidation.DeserializeOrThrow<WordExplainRequest>(message, _logger);

            if (string.IsNullOrWhiteSpace(payload.Word))
            {
                throw new NonRetryableException("Word is required.");
            }

            if (string.IsNullOrWhiteSpace(payload.Context))
            {
                throw new NonRetryableException("Context is required.");
            }

            _logger.LogInformation("Processing WordExplain for word '{Word}'", payload.Word);
            var userDetails = await _accessorClient.GetUserAsync(payload.UserId, cancellationToken);
            var lang = userDetails?.PreferredLanguageCode.ToString() ?? "en";
            var result = await _wordExplainService.ExplainAsync(payload, lang, cancellationToken);
            var response = new WordExplainResponseDto
            {
                Id = payload.Id,
                Definition = result.Definition,
                Explanation = result.Explanation,
            };

            await _publisher.SendExplainMessageAsync(
                payload.UserId.ToString(),
                response,
                message.ActionName,
                cancellationToken);

            _logger.LogInformation("WordExplain completed for word '{Word}'", payload.Word);
        }
        catch (NonRetryableException ex)
        {
            _logger.LogError(ex, "Non-retryable error processing message {Action}", message.ActionName);
            throw;
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operation cancelled while processing message {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, cancellationToken);
            }

            _logger.LogError(ex, "Transient error while processing {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing.", ex);
        }
    }
}