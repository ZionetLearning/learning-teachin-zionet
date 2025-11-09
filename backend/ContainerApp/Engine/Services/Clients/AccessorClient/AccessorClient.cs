using System.Net;
using System.Text.Json;
using Dapr.Client;
using Engine.Models.Prompts;
using Engine.Options;
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

    public async Task<AttemptDetailsResponse> GetLastAttemptAsync(Guid userId, string gameType, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting last attempt for UserId {UserId}", userId);

        try
        {
            var response = await _daprClient.InvokeMethodAsync<AttemptDetailsResponse>(
                HttpMethod.Get,
                "accessor",
                $"games-accessor/attempt/last/{userId:D}/{gameType}",
                cancellationToken: ct);

            return response;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No attempts found for UserId {UserId}", userId);
            throw new InvalidOperationException($"No attempts found for user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get last attempt for UserId {UserId}", userId);
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

    public async Task<PromptResponse?> GetPromptAsync(PromptConfiguration config, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (string.IsNullOrWhiteSpace(config.Key))
        {
            throw new ArgumentException("PromptConfiguration.Key cannot be empty", nameof(config));
        }

        _logger.LogInformation("Getting prompt {PromptKey} (version: {Version}, label: {Label})",
            config.Key, config.Version, config.Label);

        try
        {
            var queryParams = new List<string>();
            if (config.Version.HasValue)
            {
                queryParams.Add($"version={config.Version.Value}");
            }

            if (!string.IsNullOrWhiteSpace(config.Label))
            {
                queryParams.Add($"label={Uri.EscapeDataString(config.Label)}");
            }

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
            var url = $"prompts/{Uri.EscapeDataString(config.Key)}{queryString}";

            var prompt = await _daprClient.InvokeMethodAsync<PromptResponse>(
                HttpMethod.Get,
                "accessor",
                url,
                cancellationToken: ct);

            if (prompt != null)
            {
                _logger.LogInformation("Retrieved prompt {PromptKey} from source: {Source}",
                    prompt.PromptKey, prompt.Source ?? "Unknown");
            }

            return prompt;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Prompt {PromptKey} not found", config.Key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get prompt {PromptKey}", config.Key);
            throw;
        }
    }

    public async Task<GetPromptsBatchResponse> GetPromptsBatchAsync(IEnumerable<PromptConfiguration> configs, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(configs);

        var configList = configs.ToList();
        if (configList.Count == 0)
        {
            throw new ArgumentException("At least one prompt configuration must be provided", nameof(configs));
        }

        const int maxBatch = 100;
        if (configList.Count > maxBatch)
        {
            throw new ArgumentException($"Maximum {maxBatch} prompt configurations allowed. Received {configList.Count}.", nameof(configs));
        }

        _logger.LogInformation("Batch requesting {Count} prompts", configList.Count);

        var request = new GetPromptsBatchRequest
        {
            Prompts = configList
        };

        try
        {
            var response = await _daprClient.InvokeMethodAsync<GetPromptsBatchRequest, GetPromptsBatchResponse>(
                HttpMethod.Post,
                "accessor",
                "prompts/batch",
                request,
                cancellationToken: ct);

            foreach (var prompt in response.Prompts)
            {
                _logger.LogDebug("Batch: Retrieved prompt {PromptKey} from source: {Source}",
                    prompt.PromptKey, prompt.Source ?? "Unknown");
            }

            _logger.LogInformation("Batch prompt retrieval done. Found {Found} Missing {Missing}",
                response.Prompts.Count, response.NotFound.Count);

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
    public async Task<UserData?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            return await _daprClient.InvokeMethodAsync<UserData?>(
                HttpMethod.Get, AppIds.Accessor, $"users-accessor/{userId}", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return null;
        }
    }
}