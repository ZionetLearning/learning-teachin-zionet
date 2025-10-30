using System.Net;
using System.Text.Json;
using Dapr.Client;
using Engine.Models.Prompts;
using Engine.Services.Clients.AccessorClient.Models;
using Engine.Constants;
using Engine.Models.Users;

namespace Engine.Services.Clients.AccessorClient;

public class AccessorClient(ILogger<AccessorClient> logger, DaprClient daprClient) : IAccessorClient
{
    private readonly ILogger<AccessorClient> _logger = logger;
    private readonly DaprClient _daprClient = daprClient;

    public async Task<IReadOnlyList<ChatMessage>> GetChatHistoryAsync(Guid threadId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetChatHistoryAsync), nameof(AccessorClient));
        try
        {
            var messages = await _daprClient.InvokeMethodAsync<List<ChatMessage>>(
                HttpMethod.Get,
                "accessor",
                $"chats/{threadId}/messages",
                cancellationToken: ct
            );

            return messages ?? new List<ChatMessage>(); //think what return? 
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Thread {ThreadId} not found, returning empty history", threadId);
            return Array.Empty<ChatMessage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get chat history for thread {ThreadId}", threadId);
            throw;
        }
    }

    public async Task<HistorySnapshotDto> GetHistorySnapshotAsync(Guid threadId, Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("GET history snapshot for thread {ThreadId}", threadId);

        try
        {
            var dto = await _daprClient.InvokeMethodAsync<HistorySnapshotDto>(
                HttpMethod.Get, "accessor", $"chats-accessor/{threadId}/{userId}/history", cancellationToken: ct);

            return dto;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("History snapshot for thread {ThreadId} not found, returning empty", threadId);

            using var doc = JsonDocument.Parse("""{"messages":[]}""");
            return new HistorySnapshotDto
            {
                ThreadId = threadId,
                UserId = userId,
                Name = "defaultName",
                ChatType = "default",
                History = doc.RootElement.Clone()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get history snapshot for chatId: {ChatId}", threadId);
            throw;
        }
    }

    public async Task<HistorySnapshotDto> UpsertHistorySnapshotAsync(UpsertHistoryRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("UPSERT history snapshot for chatId: {ChatId}", request.ThreadId);

        try
        {
            var dto = await _daprClient.InvokeMethodAsync<UpsertHistoryRequest, HistorySnapshotDto>(
                HttpMethod.Post, "accessor", "chats-accessor/history", request, cancellationToken: ct);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert history snapshot for chatId {ChatId}", request.ThreadId);
            throw;
        }
    }

    public async Task<AttemptDetailsResponse> GetAttemptDetailsAsync(Guid userId, Guid attemptId, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting attempt details for UserId {UserId}, AttemptId {AttemptId}", userId, attemptId);

        try
        {
            var response = await _daprClient.InvokeMethodAsync<AttemptDetailsResponse>(
                HttpMethod.Get,
                "accessor",
                $"games-accessor/attempt/{userId:D}/{attemptId:D}",
                cancellationToken: ct);

            return response;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Attempt not found for UserId {UserId}, AttemptId {AttemptId}", userId, attemptId);
            throw new InvalidOperationException($"Attempt {attemptId} not found for user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get attempt details for UserId {UserId}, AttemptId {AttemptId}", userId, attemptId);
            throw;
        }
    }

    // ========== PROMPTS ==========

    public async Task<PromptResponse> CreatePromptAsync(CreatePromptRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.PromptKey))
        {
            throw new ArgumentException("PromptKey is required", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Content is required", nameof(request));
        }

        _logger.LogInformation("Creating prompt {PromptKey}", request.PromptKey);

        try
        {
            // POST /prompts/
            var response = await _daprClient.InvokeMethodAsync<CreatePromptRequest, PromptResponse>(
                HttpMethod.Post,
                "accessor",
                "prompts",
                request,
                cancellationToken: ct);

            _logger.LogInformation("Created prompt {PromptKey}", response.PromptKey);
            return response;
        }
        catch (InvocationException ex)
        {
            if (ex.Response is not null)
            {
                _logger.LogWarning(ex, "Accessor returned {Status} creating prompt {PromptKey}", ex.Response.StatusCode, request.PromptKey);
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating prompt {PromptKey}", request.PromptKey);
            throw;
        }
    }

    public async Task<PromptResponse?> GetPromptAsync(string promptKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(promptKey))
        {
            throw new ArgumentException("promptKey cannot be empty", nameof(promptKey));
        }

        _logger.LogInformation("Getting latest prompt {PromptKey}", promptKey);

        try
        {
            var prompt = await _daprClient.InvokeMethodAsync<PromptResponse>(
                HttpMethod.Get,
                "accessor",
                $"prompts/{Uri.EscapeDataString(promptKey)}",
                cancellationToken: ct);

            return prompt;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Prompt {PromptKey} not found", promptKey);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get prompt {PromptKey}", promptKey);
            throw;
        }
    }

    public async Task<GetPromptsBatchResponse> GetPromptsBatchAsync(IEnumerable<string> promptKeys, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(promptKeys);

        var keys = promptKeys
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (keys.Count == 0)
        {
            throw new ArgumentException("At least one prompt key must be provided", nameof(promptKeys));
        }

        const int maxBatch = 100;
        if (keys.Count > maxBatch)
        {
            throw new ArgumentException($"Maximum {maxBatch} prompt keys allowed. Received {keys.Count}.", nameof(promptKeys));
        }

        _logger.LogInformation("Batch requesting {Count} prompts", keys.Count);

        var request = new GetPromptsBatchRequest
        {
            PromptKeys = keys
        };

        try
        {
            var response = await _daprClient.InvokeMethodAsync<GetPromptsBatchRequest, GetPromptsBatchResponse>(
                HttpMethod.Post,
                "accessor",
                "prompts/batch",
                request,
                cancellationToken: ct);

            _logger.LogInformation("Batch prompt retrieval done. Found {Found} Missing {Missing}", response.Prompts.Count, response.NotFound.Count);
            return response;
        }
        catch (InvocationException ex)
        {
            if (ex.Response?.StatusCode == HttpStatusCode.BadRequest)
            {
                _logger.LogWarning(ex, "Bad batch prompt request");
                throw new ArgumentException("Invalid batch prompt request", ex);
            }

            _logger.LogError(ex, "Invocation failure for batch prompt retrieval");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving batch prompts");
            throw;
        }
    }

    public async Task<IReadOnlyList<PromptResponse>> GetPromptVersionsAsync(string promptKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(promptKey))
        {
            throw new ArgumentException("promptKey cannot be empty", nameof(promptKey));
        }

        _logger.LogInformation("Getting all versions for prompt {PromptKey}", promptKey);

        try
        {
            var versions = await _daprClient.InvokeMethodAsync<List<PromptResponse>>(
                HttpMethod.Get,
                "accessor",
                $"prompts/{Uri.EscapeDataString(promptKey)}/versions",
                cancellationToken: ct);

            return versions ?? new List<PromptResponse>();
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("No versions found for prompt {PromptKey}", promptKey);
            return Array.Empty<PromptResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get prompt versions {PromptKey}", promptKey);
            throw;
        }
    }

    public async Task<List<string>> GetUserInterestsAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var userInterests = await _daprClient.InvokeMethodAsync<List<string>>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"users-accessor/{userId}/interests",
                ct);

            if (userInterests == null)
            {
                _logger.LogWarning("User {UserId} not found when fetching interests.", userId);
                return [];
            }

            return userInterests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching interests for user {UserId}", userId);
            return [];
        }
    }
    public async Task<UserData?> GetUserAsync(Guid userId)
    {
        try
        {
            return await _daprClient.InvokeMethodAsync<UserData?>(
                HttpMethod.Get, AppIds.Accessor, $"users-accessor/{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return null;
        }
    }
}