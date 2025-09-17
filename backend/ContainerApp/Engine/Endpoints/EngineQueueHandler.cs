using System.Diagnostics;
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
        .On(MessageAction.GenerateSentences, HandleSentenceGenerationAsync)
        .On(MessageAction.GenerateSplitSentences, HandleSentenceGenerationAsync);

    public EngineQueueHandler(
        DaprClient daprClient,
        IChatAiService aiService,
        IAiReplyPublisher publisher,
        IAccessorClient accessorClient,
        ISentencesService sentencesService,
        IChatTitleService chatTitleService,
        ILogger<EngineQueueHandler> logger) : base(logger)
    {
        _daprClient = daprClient;
        _aiService = aiService;
        _publisher = publisher;
        _accessorClient = accessorClient;
        _chatTitleService = chatTitleService;
        _logger = logger;
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

            var chatName = snapshot.Name;
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

            var serviceRequest = new ChatAiServiseRequest
            {
                History = storyForKernel,
                ChatType = request.ChatType,
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                RequestId = request.RequestId,
                SentAt = request.SentAt,
                TtlSeconds = request.TtlSeconds,
            };

            var aiResponse = await _aiService.ChatHandlerAsync(serviceRequest, ct);

            afterChatServiseTime = sw.ElapsedMilliseconds;

            if (aiResponse.Status != ChatAnswerStatus.Ok || aiResponse.Answer is null)
            {
                var errorResponse = new EngineChatResponse
                {
                    RequestId = serviceRequest.RequestId,
                    Status = ChatAnswerStatus.Fail,
                    ChatName = chatName,
                    ThreadId = serviceRequest.ThreadId,
                    AssistantMessage = aiResponse.Error
                };

                await _publisher.SendReplyAsync(userContext, errorResponse, ct);
                _logger.LogError("Chat request {RequestId} failed: {Error}", aiResponse.RequestId, aiResponse.Error);
                return;
            }

            HistoryMapper.AppendDelta(storyForKernel, aiResponse.UpdatedHistory);

            var upsertFinal = new UpsertHistoryRequest
            {
                ThreadId = request.ThreadId,
                UserId = request.UserId,
                Name = chatName,
                ChatType = request.ChatType.ToString().ToLowerInvariant(),
                History = HistoryMapper.SerializeHistory(storyForKernel)
            };

            await _accessorClient.UpsertHistorySnapshotAsync(upsertFinal, ct);

            var responseToManager = new EngineChatResponse
            {
                AssistantMessage = aiResponse.Answer.Content,
                RequestId = aiResponse.RequestId,
                ChatName = chatName,
                Status = aiResponse.Status,
                ThreadId = aiResponse.ThreadId
            };

            await _publisher.SendReplyAsync(userContext, responseToManager, ct);
            _logger.LogInformation("Chat request {RequestId} processed successfully", aiResponse.RequestId);
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
            if (request is not null)
            {

                _logger.LogInformation("Chat request {RequestId} chatId {ThreadId} userId {UserId}, " +
                    "getHistoryTime {GetHistoryTime} ms, addOrCheckSystemPromptTime {AddOrCheckSystemPromptTime} ms, addOrCheckChatNameTime {AddOrCheckChatNameTime}  ms, " +
                    "afterChatServiseTime {AfterChatServiseTime} ms, finished in {ElapsedMs} ms",
                    request.RequestId,
                    request.ThreadId,
                    request.UserId,
                    getHistoryTime,
                    addOrCheckSystemPromptTime,
                    addOrCheckChatNameTime,
                    afterChatServiseTime,
                    sw.ElapsedMilliseconds);
            }
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
