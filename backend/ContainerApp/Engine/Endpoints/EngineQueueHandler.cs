using System.Diagnostics;
using System.Text;
using Dapr.Client;
using DotQueue;
using Engine.Constants;
using Engine.Helpers;
using Engine.Models;
using Engine.Models.Chat;
using Engine.Models.QueueMessages;
using Engine.Models.Sentences;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Engine.Services.Clients.AccessorClient.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

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
    protected override MessageAction GetAction(Message message) => message.ActionName;

    protected override void Configure(RouteBuilder routes) => routes
        .On(MessageAction.CreateTask, HandleCreateTaskAsync)
        .On(MessageAction.TestLongTask, HandleTestLongTaskAsync)
        .On(MessageAction.ProcessingChatMessage, HandleProcessingChatMessageAsync)
        .On(MessageAction.ProcessingExplainMistake, HandleProcessingExplainMistakeAsync)
        .On(MessageAction.GenerateSentences, HandleSentenceGenerationAsync)
        .On(MessageAction.GenerateSplitSentences, HandleSentenceGenerationAsync);

    public EngineQueueHandler(
        DaprClient daprClient,
        ILogger<EngineQueueHandler> logger,
        ILogger<StreamingChatAIBatcher> batcherLogger,
        IChatAiService aiService,
        IAiReplyPublisher publisher,
        IAccessorClient accessorClient,
        ISentencesService sentencesService,
        IChatTitleService chatTitleService) : base(logger)
    {
        _daprClient = daprClient;
        _logger = logger;
        _batcherLogger = batcherLogger;
        _aiService = aiService;
        _publisher = publisher;
        _accessorClient = accessorClient;
        _chatTitleService = chatTitleService;
        _sentencesService = sentencesService;
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

    private async Task HandleProcessingChatMessageAsync(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken ct)
    {
        long getHistoryTime = 0;
        long addOrCheckSystemPromptTime = 0;
        long addOrCheckChatNameTime = 0;
        long afterChatServiseTime = 0;

        var sw = Stopwatch.StartNew();
        EngineChatRequest? request = null;
        var chatName = string.Empty;
        ChatAiServiseRequest? serviceRequest = null;
        CancellationTokenSource? renewalCts = null;
        Task? renewTask = null;
        Stopwatch? elapsed = null;
        var seq = 0;
        Func<int> NextSeq = () => Interlocked.Increment(ref seq) - 1;
        try
        {
            request = PayloadValidation.DeserializeOrThrow<EngineChatRequest>(message, _logger);
            PayloadValidation.ValidateEngineChatRequest(request, _logger);

            var userContext = MetadataValidation.DeserializeOrThrow<UserContextMetadata>(message, _logger);
            MetadataValidation.ValidateUserContext(userContext, _logger);

            using var _ = _logger.BeginScope(new { request.RequestId, request.ThreadId, request.UserId });

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

            var snapshot = await _accessorClient.GetHistorySnapshotAsync(request.ThreadId, request.UserId, ct);

            getHistoryTime = sw.ElapsedMilliseconds;
            var skHistory = HistoryMapper.ToChatHistoryFromElement(snapshot.History);
            var storyForKernel = HistoryMapper.CloneToChatHistory(skHistory);

            if (!storyForKernel.Any(m => m.Role == AuthorRole.System))
            {
                var systemPrompt = await GetOrLoadSystemPromptAsync(ct);
                storyForKernel.Insert(0, new ChatMessageContent(AuthorRole.System, systemPrompt));
            }

            addOrCheckSystemPromptTime = sw.ElapsedMilliseconds;

            storyForKernel.AddUserMessage(request.UserMessage.Trim(), DateTimeOffset.UtcNow);

            chatName = snapshot.Name;
            if (chatName == "New chat")
            {
                try
                {
                    chatName = await _chatTitleService.GenerateTitleAsync(storyForKernel, ct);
                }
                catch (Exception exName)
                {
                    _logger.LogError(exName, "Error while processing naming chat: {RequestId}", request.RequestId);
                    chatName = DateTime.UtcNow.ToString("HHmm_dd_MM");
                }
            }

            addOrCheckChatNameTime = sw.ElapsedMilliseconds;

            var upsertUserMessage = new UpsertHistoryRequest
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Name = chatName,
                ChatType = request.ChatType.ToString().ToLowerInvariant(),
                History = HistoryMapper.SerializeHistory(storyForKernel)
            };

            await _accessorClient.UpsertHistorySnapshotAsync(upsertUserMessage, ct);

            serviceRequest = new ChatAiServiseRequest
            {
                History = storyForKernel,
                ChatType = request.ChatType,
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                RequestId = request.RequestId,
                SentAt = request.SentAt,
                TtlSeconds = request.TtlSeconds,
            };

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

            EngineChatStreamResponse BuildResponse(
                ChatStreamStage stage,
                string? delta = null,
                string? toolCall = null,
                string? toolResult = null)
            {
                return new EngineChatStreamResponse
                {
                    RequestId = serviceRequest.RequestId,
                    ThreadId = serviceRequest.ThreadId,
                    UserId = serviceRequest.UserId,
                    ChatName = chatName,
                    Stage = stage,
                    Delta = delta,
                    ToolCall = toolCall,
                    ToolResult = toolResult,
                    IsFinal = false,
                    ElapsedMs = elapsed.ElapsedMilliseconds
                };
            }

            var total = new StringBuilder();

            await using var batcher = new StreamingChatAIBatcher(
                minChars: 80,
                maxLatency: TimeSpan.FromMilliseconds(250),
                makeChunk: (batchedText) => BuildResponse(ChatStreamStage.Model, delta: batchedText),
                sendAsync: async (chunk) =>
                {
                    chunk.Sequence = NextSeq();

                    if (chunk.Stage == ChatStreamStage.Model && !string.IsNullOrEmpty(chunk.Delta))
                    {
                        total.Append(chunk.Delta);

                    }

                    await _publisher.SendStreamAsync(userContext, chunk, ct);
                },
                makeToolChunk: (upd) => BuildResponse(ChatStreamStage.Tool, toolCall: upd.ToolCall),
                makeToolResultChunk: (upd) => BuildResponse(ChatStreamStage.ToolResult, toolResult: upd.ToolResult),
                logger: _batcherLogger,
                ct: ct);

            await foreach (var upd in _aiService.ChatStreamAsync(serviceRequest, ct))
            {
                await batcher.HandleUpdateAsync(upd);

                if (upd.IsFinal)
                {
                    HistoryMapper.AppendDelta(storyForKernel, upd.UpdatedHistory);
                    break;
                }
            }

            await batcher.FlushAsync();

            var finalAnswer = total.ToString();

            await _accessorClient.UpsertHistorySnapshotAsync(new UpsertHistoryRequest
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Name = chatName,
                ChatType = request.ChatType.ToString().ToLowerInvariant(),
                History = HistoryMapper.SerializeHistory(storyForKernel)
            }, ct);

            var finalChunk = new EngineChatStreamResponse
            {
                RequestId = serviceRequest.RequestId,
                ThreadId = serviceRequest.ThreadId,
                UserId = serviceRequest.UserId,
                ChatName = chatName,
                Sequence = NextSeq(),
                Delta = finalAnswer,
                Stage = ChatStreamStage.Final,
                IsFinal = true,
                ElapsedMs = elapsed.ElapsedMilliseconds
            };

            await _publisher.SendStreamAsync(userContext, finalChunk, ct);

            afterChatServiseTime = sw.ElapsedMilliseconds;

            _logger.LogInformation("Chat request {RequestId} processed successfully", request.RequestId);
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            var ms = elapsed?.ElapsedMilliseconds ?? 0;
            var canceled = new EngineChatStreamResponse
            {
                RequestId = serviceRequest?.RequestId ?? request?.RequestId ?? string.Empty,
                ThreadId = serviceRequest?.ThreadId ?? request?.ThreadId ?? Guid.Empty,
                UserId = serviceRequest?.UserId ?? request?.UserId ?? Guid.Empty,
                ChatName = string.IsNullOrWhiteSpace(chatName) ? "Chat" : chatName,
                Sequence = NextSeq(),
                Stage = ChatStreamStage.Canceled,
                IsFinal = true,
                ElapsedMs = ms
            };

            _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);

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
                _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, ct);
            }

            _logger.LogError(ex, "Transient error while processing AI chat {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing AI chat.", ex);
        }
        finally
        {
            sw.Stop();

            _logger.LogInformation(
                "Chat request {RequestId} chatId {ThreadId} userId {UserId}, getHistoryTime {GetHistoryTime} ms, " +
                "addOrCheckSystemPromptTime {AddOrCheckSystemPromptTime} ms, addOrCheckChatNameTime {AddOrCheckChatNameTime} ms, " +
                "afterChatServiseTime {AfterChatServiseTime} ms, finished in {ElapsedMs} ms",
                request?.RequestId,
                request?.ThreadId,
                request?.UserId,
                getHistoryTime,
                addOrCheckSystemPromptTime,
                addOrCheckChatNameTime,
                afterChatServiseTime,
                sw.ElapsedMilliseconds);

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

    private async Task HandleProcessingExplainMistakeAsync(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken ct)
    {
        long getAttemptDetailsTime = 0;
        long getSystemPromptTime = 0;
        long getMistakePromptTime = 0;
        long afterChatServicesTime = 0;

        var sw = Stopwatch.StartNew();
        EngineExplainMistakeRequest? request = null;
        var chatName = string.Empty;
        ChatAiServiseRequest? serviceRequest = null;
        CancellationTokenSource? renewalCts = null;
        Task? renewTask = null;
        Stopwatch? elapsed = null;
        var seq = 0;
        Func<int> NextSeq = () => Interlocked.Increment(ref seq) - 1;

        try
        {
            request = PayloadValidation.DeserializeOrThrow<EngineExplainMistakeRequest>(message, _logger);
            PayloadValidation.ValidateEngineExplainMistakeRequest(request, _logger);

            var userContext = MetadataValidation.DeserializeOrThrow<UserContextMetadata>(message, _logger);
            MetadataValidation.ValidateUserContext(userContext, _logger);

            using var _ = _logger.BeginScope(new { request.RequestId, request.ThreadId, request.UserId, request.AttemptId });

            if (request.UserId == Guid.Empty)
            {
                _logger.LogWarning("UserId is empty for explain mistake request {RequestId}", request.RequestId);
                throw new NonRetryableException("UserId is required.");
            }

            if (request.TtlSeconds <= 0)
            {
                _logger.LogWarning("TtlSeconds is invalid for explain mistake request {RequestId}: {TtlSeconds}", request.RequestId, request.TtlSeconds);
                throw new NonRetryableException("TtlSeconds must be greater than 0.");
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now > request.SentAt + request.TtlSeconds)
            {
                _logger.LogWarning("Explain mistake request {RequestId} expired. Skipping.", request.RequestId);
                throw new NonRetryableException("Request TTL expired.");
            }

            _logger.LogInformation("Fetching attempt details for AttemptId {AttemptId}", request.AttemptId);
            var attemptDetails = await _accessorClient.GetAttemptDetailsAsync(request.UserId, request.AttemptId, ct);
            getAttemptDetailsTime = sw.ElapsedMilliseconds;

            var storyForKernel = new ChatHistory();
            var systemPrompt = await _accessorClient.GetPromptAsync(PromptsKeys.ExplainMistakeSystem, ct);
            if (systemPrompt?.Content is not null)
            {
                storyForKernel.Insert(0, new ChatMessageContent(AuthorRole.System, systemPrompt.Content));
            }
            else
            {
                _logger.LogWarning("System prompt for explain mistake not found, using fallback");
                storyForKernel.Insert(0, new ChatMessageContent(AuthorRole.System,
                    "You are a helpful Hebrew language tutor. Explain mistakes clearly and provide educational guidance."));
            }

            getSystemPromptTime = sw.ElapsedMilliseconds;
            var mistakeExplanationPrompt = await BuildMistakeExplanationPromptAsync(attemptDetails, request.GameType, ct);
            storyForKernel.AddUserMessage(mistakeExplanationPrompt, DateTimeOffset.UtcNow);

            getMistakePromptTime = sw.ElapsedMilliseconds;

            chatName = $"Mistake Explanation - {request.GameType}";

            serviceRequest = new ChatAiServiseRequest
            {
                History = storyForKernel,
                ChatType = request.ChatType,
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                RequestId = request.RequestId,
                SentAt = request.SentAt,
                TtlSeconds = request.TtlSeconds,
            };

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
                    _logger.LogWarning(ex, "Renew lock loop failed during explain mistake streaming");
                }
            }, renewalCts.Token);

            elapsed = Stopwatch.StartNew();

            EngineChatStreamResponse BuildResponse(
                ChatStreamStage stage,
                string? delta = null,
                string? toolCall = null,
                string? toolResult = null)
            {
                return new EngineChatStreamResponse
                {
                    RequestId = serviceRequest.RequestId,
                    ThreadId = serviceRequest.ThreadId,
                    UserId = serviceRequest.UserId,
                    ChatName = chatName,
                    Stage = stage,
                    Delta = delta,
                    ToolCall = toolCall,
                    ToolResult = toolResult,
                    IsFinal = false,
                    ElapsedMs = elapsed.ElapsedMilliseconds
                };
            }

            var total = new StringBuilder();

            await using var batcher = new StreamingChatAIBatcher(
                minChars: 80,
                maxLatency: TimeSpan.FromMilliseconds(250),
                makeChunk: (batchedText) => BuildResponse(ChatStreamStage.Model, delta: batchedText),
                sendAsync: async (chunk) =>
                {
                    chunk.Sequence = NextSeq();

                    if (chunk.Stage == ChatStreamStage.Model && !string.IsNullOrEmpty(chunk.Delta))
                    {
                        total.Append(chunk.Delta);
                    }

                    await _publisher.SendStreamAsync(userContext, chunk, ct);
                },
                makeToolChunk: (upd) => BuildResponse(ChatStreamStage.Tool, toolCall: upd.ToolCall),
                makeToolResultChunk: (upd) => BuildResponse(ChatStreamStage.ToolResult, toolResult: upd.ToolResult),
                logger: _batcherLogger,
                ct: ct);

            await foreach (var upd in _aiService.ChatStreamAsync(serviceRequest, ct))
            {
                await batcher.HandleUpdateAsync(upd);

                if (upd.IsFinal)
                {
                    HistoryMapper.AppendDelta(storyForKernel, upd.UpdatedHistory);
                    break;
                }
            }

            await batcher.FlushAsync();

            var finalAnswer = total.ToString();

            await _accessorClient.UpsertHistorySnapshotAsync(new UpsertHistoryRequest
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Name = chatName,
                ChatType = request.ChatType.ToString().ToLowerInvariant(),
                History = HistoryMapper.SerializeHistory(storyForKernel)
            }, ct);

            var finalChunk = new EngineChatStreamResponse
            {
                RequestId = serviceRequest.RequestId,
                ThreadId = serviceRequest.ThreadId,
                UserId = serviceRequest.UserId,
                ChatName = chatName,
                Sequence = NextSeq(),
                Delta = finalAnswer,
                Stage = ChatStreamStage.Final,
                IsFinal = true,
                ElapsedMs = elapsed.ElapsedMilliseconds
            };

            await _publisher.SendStreamAsync(userContext, finalChunk, ct);

            afterChatServicesTime = sw.ElapsedMilliseconds;

            _logger.LogInformation("Explain mistake request {RequestId} processed successfully", request.RequestId);
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            var ms = elapsed?.ElapsedMilliseconds ?? 0;
            var canceled = new EngineChatStreamResponse
            {
                RequestId = serviceRequest?.RequestId ?? request?.RequestId ?? string.Empty,
                ThreadId = serviceRequest?.ThreadId ?? request?.ThreadId ?? Guid.Empty,
                UserId = serviceRequest?.UserId ?? request?.UserId ?? Guid.Empty,
                ChatName = string.IsNullOrWhiteSpace(chatName) ? "Explain Mistake" : chatName,
                Sequence = NextSeq(),
                Stage = ChatStreamStage.Canceled,
                IsFinal = true,
                ElapsedMs = ms
            };

            _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);

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
                _logger.LogWarning(ex, "Operation cancelled while processing {Action}", message.ActionName);
                throw new OperationCanceledException("Operation was cancelled.", ex, ct);
            }

            _logger.LogError(ex, "Transient error while processing explain mistake {Action}", message.ActionName);
            throw new RetryableException("Transient error while processing explain mistake.", ex);
        }
        finally
        {
            sw.Stop();

            _logger.LogInformation(
                "Explain mistake request {RequestId} chatId {ThreadId} userId {UserId}, attemptId {AttemptId}, " +
                "getAttemptDetailsTime {GetAttemptDetailsTime} ms, getSystemPromptTime {GetSystemPromptTime} ms, " +
                "getMistakePromptTime {GetMistakePromptTime} ms, afterChatServiseTime {AfterChatServiseTime} ms, " +
                "finished in {ElapsedMs} ms",
                request?.RequestId,
                request?.ThreadId,
                request?.UserId,
                request?.AttemptId,
                getAttemptDetailsTime,
                getSystemPromptTime,
                getMistakePromptTime,
                afterChatServicesTime,
                sw.ElapsedMilliseconds);

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

    private async Task<string> BuildMistakeExplanationPromptAsync(AttemptDetailsResponse attemptDetails, string gameType, CancellationToken ct)
    {
        var userAnswerText = string.Join(" ", attemptDetails.GivenAnswer);
        var correctAnswerText = string.Join(" ", attemptDetails.CorrectAnswer);

        var mistakeTemplatePrompt = await _accessorClient.GetPromptAsync(PromptsKeys.MistakeTemplate, ct);

        if (mistakeTemplatePrompt?.Content is not null)
        {
            return mistakeTemplatePrompt.Content
                .Replace("{gameType}", gameType)
                .Replace("{difficulty}", attemptDetails.Difficulty)
                .Replace("{userAnswer}", userAnswerText)
                .Replace("{correctAnswer}", correctAnswerText);
        }
        else
        {
            _logger.LogWarning("Mistake explanation template not found in database, using fallback");
            return $"""
                Please explain the mistake in this {gameType} exercise:

                **Exercise Details:**
                - Game Type: {gameType}
                - Difficulty: {attemptDetails.Difficulty}

                **Student's Answer:** {userAnswerText}
                **Correct Answer:** {correctAnswerText}

                Please provide a clear, educational explanation of:
                1. What the mistake was
                2. Why the correct answer is right
                3. Tips to avoid this mistake in the future

                Be encouraging and focus on learning rather than just pointing out the error.
                """;
        }
    }

    private async Task<string> GetOrLoadSystemPromptAsync(CancellationToken ct)
    {

        var keys = new[] { PromptsKeys.SystemDefault, PromptsKeys.DetailedExplanation };
        var fallback = "You are a helpful assistant. Keep answers concise.";

        try
        {
            var batch = await _accessorClient.GetPromptsBatchAsync(keys, ct);
            var map = batch.Prompts.ToDictionary(p => p.PromptKey, p => p.Content, StringComparer.Ordinal);

            var combined = string.Join(
                "\n\n",
                keys.Select(k => map.TryGetValue(k, out var v) ? v : null)
                    .Where(v => !string.IsNullOrWhiteSpace(v)));

            if (string.IsNullOrWhiteSpace(combined))
            {
                _logger.LogWarning("System prompt batch returned empty; using fallback.");
                combined = fallback;
            }

            if (batch.NotFound?.Count > 0)
            {
                _logger.LogWarning("Missing prompt keys: {Keys}", string.Join(",", batch.NotFound));
            }

            return combined;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed retrieving system prompts; using fallback.");
            return fallback;
        }
    }

    private async Task HandleSentenceGenerationAsync(Message message, IReadOnlyDictionary<string, string>? metadata, Func<Task> renewLock, CancellationToken cancellationToken)
    {
        try
        {
            var payload = PayloadValidation.DeserializeOrThrow<SentenceRequest>(message, _logger);

            PayloadValidation.ValidateSentenceGenerationRequest(payload, _logger);

            _logger.LogDebug("Processing sentence generation");
            var response = await _sentencesService.GenerateAsync(payload, cancellationToken);
            var userId = payload.UserId;
            await _publisher.SendGeneratedMessagesAsync(userId.ToString(), response, message.ActionName, cancellationToken);
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
}
