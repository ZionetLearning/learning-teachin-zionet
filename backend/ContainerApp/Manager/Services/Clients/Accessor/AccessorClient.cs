using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using Manager.Models.Auth;
using Manager.Models.Auth.RefreshSessions;
using Manager.Models.Chat;
using Manager.Models.UserGameConfiguration;
using Manager.Models.QueueMessages;
using Manager.Models.Users;
using Manager.Models.WordCards;
using Manager.Services.Clients.Accessor.Models;
using Manager.Services.Clients.Accessor.Interfaces;

namespace Manager.Services.Clients.Accessor;

public class AccessorClient(
    ILogger<AccessorClient> logger,
    DaprClient daprClient,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper
    ) : IAccessorClient
{
    private readonly ILogger<AccessorClient> _logger = logger;
    private readonly DaprClient _daprClient = daprClient;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IMapper _mapper = mapper;

    public async Task<TaskModel?> GetTaskAsync(int id)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetTaskAsync), nameof(AccessorClient));
        try
        {
            var task = await _daprClient.InvokeMethodAsync<TaskModel?>(
                HttpMethod.Get, AppIds.Accessor, $"tasks-accessor/task/{id}");
            _logger.LogDebug("Received task {TaskId} from Accessor service", id);
            return task;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Task with ID {TaskId} not found (404 from accessor)", id);
            return null; // treat 404 as "not found", not exception
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get task {TaskId} from Accessor service", id);
            throw;
        }
    }
    public async Task<int> CleanupRefreshSessionsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(CleanupRefreshSessionsAsync), nameof(AccessorClient));
        try
        {
            var resp = await _daprClient.InvokeMethodAsync<CleanupResponse>(
                HttpMethod.Post,
                AppIds.Accessor,
                "auth-accessor/refresh-sessions/internal/cleanup",
                ct);

            return resp?.Deleted ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke cleanup on Accessor");
            throw;
        }
    }

    public async Task<bool> UpdateTaskName(int id, string newTaskName)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(UpdateTaskName), nameof(AccessorClient));
        try
        {
            var task = await GetTaskAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task {TaskId} not found, cannot update name.", id);
                return false;
            }

            var payload = JsonSerializer.SerializeToElement(new
            {
                id,
                name = newTaskName,
                payload = ""
            });

            var message = new Message
            {
                ActionName = MessageAction.UpdateTask,
                Payload = payload
            };

            await _daprClient.InvokeBindingAsync(
                $"{QueueNames.AccessorQueue}-out",
                "create",
                message
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task name update to queue for task {TaskId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteTask(int id)
    {
        _logger.LogInformation(
            "Inside: {Method} in {Class}",
            nameof(DeleteTask),
            nameof(AccessorClient)
        );
        try
        {
            await _daprClient.InvokeMethodAsync(HttpMethod.Delete, AppIds.Accessor, $"tasks-accessor/task/{id}");
            _logger.LogDebug("Task {TaskId} deletion request sent to Accessor service", id);

            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Task with ID {TaskId} not found for deletion (404 from accessor)", id);
            return false; // treat 404 as "not found", not exception
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inside the DeleteUser");
            throw;
        }
    }

    public async Task<(bool success, string message)> PostTaskAsync(TaskModel task)
    {
        _logger.LogInformation(
           "Inside: {Method} in {Class}",
           nameof(PostTaskAsync),
           nameof(AccessorClient)
       );

        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out _))
            {
                _logger.LogError("Missing or invalid UserId in HttpContext.");
                throw new InvalidOperationException("Authenticated user id is missing or not a valid GUID.");
            }

            var payload = JsonSerializer.SerializeToElement(task);
            var userContextMetadata = JsonSerializer.SerializeToElement(
                new UserContextMetadata
                {
                    UserId = userId!,
                    MessageId = Guid.NewGuid().ToString()
                }
            );

            var message = new Message
            {
                ActionName = MessageAction.CreateTask,
                Payload = payload,
                Metadata = userContextMetadata
            };
            await _daprClient.InvokeBindingAsync($"{QueueNames.AccessorQueue}-out", "create", message);

            _logger.LogDebug(
                "Task {TaskId} sent to Accessor via binding '{Binding}' for user {UserId}",
                task.Id,
                QueueNames.AccessorQueue,
                userId
            );
            return (true, "sent to queue");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task {TaskId} to Accessor", task.Id);
            throw;
        }
    }

    public async Task<IReadOnlyList<ChatSummary>> GetChatsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetChatsForUserAsync), nameof(AccessorClient));

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("userId cannot be not Empty.", nameof(userId));
        }

        try
        {
            var chats = await _daprClient.InvokeMethodAsync<List<ChatSummary>>(
                HttpMethod.Get, AppIds.Accessor, $"chats-accessor/{userId}", cancellationToken: ct);

            return chats ?? new List<ChatSummary>();
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No chats found for user {UserId}", userId);
            return Array.Empty<ChatSummary>();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("{Metod} cancelled for user {UserId}", nameof(GetChatsForUserAsync), userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get chats for user {UserId}", userId);
            throw;
        }
    }

    public async Task<StatsSnapshot?> GetStatsSnapshotAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetStatsSnapshotAsync), nameof(AccessorClient));
        try
        {
            var snapshot = await _daprClient.InvokeMethodAsync<StatsSnapshot>(
                HttpMethod.Get, AppIds.Accessor, "internal-accessor/stats/snapshot", ct);
            return snapshot; // may be null if Accessor returns empty body
        }
        catch (InvocationException ex) when (
            ex.Response?.StatusCode == HttpStatusCode.NoContent ||
            ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No stats snapshot available from Accessor ({Status})", ex.Response?.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stats snapshot from Accessor");
            throw;
        }
    }

    public async Task<SpeechTokenResponse> GetSpeechTokenAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetSpeechTokenAsync), nameof(AccessorClient));
        try
        {
            var speechTokenResponse = await _daprClient.InvokeMethodAsync<SpeechTokenResponse>(
                HttpMethod.Get,
                AppIds.Accessor,
                "media-accessor/speech/token",
                ct);
            return speechTokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get speech token from Accessor");
            throw;
        }
    }

    public async Task<AuthenticatedUser?> LoginUserAsync(LoginRequest loginRequest, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(LoginUserAsync), nameof(AccessorClient));
        try
        {
            var response = await _daprClient.InvokeMethodAsync<LoginRequest, AuthenticatedUser?>(
                HttpMethod.Post,
                AppIds.Accessor,
                "auth-accessor/login",
                loginRequest,
                ct
            );
            return response;
        }
        catch (InvocationException ex) when (
            ex.Response?.StatusCode == HttpStatusCode.NoContent ||
            ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning(
                        "Login failed – received {StatusCode} from Accessor", ex.Response?.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get login from the Accessor");
            throw;
        }
    }

    public async Task SaveSessionDBAsync(RefreshSessionRequest session, CancellationToken ct)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(SaveSessionDBAsync), nameof(AccessorClient));
        try
        {
            await _daprClient.InvokeMethodAsync(
            HttpMethod.Post,
            AppIds.Accessor,
            "auth-accessor/refresh-sessions",
            session,
            ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save refresh session to Accessor");
            throw;
        }
    }

    public async Task<RefreshSessionDto> GetSessionAsync(string oldHash, CancellationToken ct)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetSessionAsync), nameof(AccessorClient));
        try
        {
            var session = await _daprClient.InvokeMethodAsync<RefreshSessionDto>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"auth-accessor/refresh-sessions/by-token-hash/{oldHash}",
                ct
            );
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get session from the accessor");
            throw;
        }
    }

    public async Task UpdateSessionDBAsync(Guid sessionId, RotateRefreshSessionRequest rotatePayload, CancellationToken ct)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(UpdateSessionDBAsync), nameof(AccessorClient));
        try
        {
            await _daprClient.InvokeMethodAsync(
            HttpMethod.Put,
            AppIds.Accessor,
            $"auth-accessor/refresh-sessions/{sessionId}/rotate",
            rotatePayload,
            ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save refresh session to Accessor");
            throw;
        }
    }

    public async Task DeleteSessionDBAsync(Guid sessionId, CancellationToken ct)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(DeleteSessionDBAsync), nameof(AccessorClient));
        try
        {
            await _daprClient.InvokeMethodAsync(
            HttpMethod.Delete,
            AppIds.Accessor,
            $"auth-accessor/refresh-sessions/{sessionId}",
            ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save refresh session to Accessor");
            throw;
        }
    }

    public async Task<(TaskModel? Task, string? ETag)> GetTaskWithEtagAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetTaskWithEtagAsync), nameof(AccessorClient));

        try
        {
            var req = _daprClient.CreateInvokeMethodRequest(HttpMethod.Get, AppIds.Accessor, $"tasks-accessor/task/{id}");

            using var resp = await _daprClient.InvokeMethodWithResponseAsync(req, ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Task {TaskId} not found at Accessor", id);
                return (null, null);
            }

            resp.EnsureSuccessStatusCode();

            var etag = resp.Headers.ETag?.Tag?.Trim('"');

            var task = await resp.Content.ReadFromJsonAsync<TaskModel>(cancellationToken: ct);

            return (task, etag);
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Task {TaskId} not found at Accessor (404)", id);
            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to GET task {TaskId} with ETag from Accessor", id);
            throw;
        }
    }
    public async Task<IReadOnlyList<TaskSummaryDto>> GetTaskSummariesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetTaskSummariesAsync), nameof(AccessorClient));
        try
        {
            var list = await _daprClient.InvokeMethodAsync<List<TaskSummaryDto>>(
                HttpMethod.Get,
                AppIds.Accessor,
                "tasks-accessor/tasks",
                ct);

            _logger.LogInformation("Accessor returned {Count} task summaries", list?.Count ?? 0);
            return list ?? new List<TaskSummaryDto>();
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Accessor returned 404 for tasks list");
            return Array.Empty<TaskSummaryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tasks list from Accessor");
            throw;
        }
    }

    public async Task<UpdateTaskNameResult> UpdateTaskNameAsync(int id, string newTaskName, string ifMatch, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(UpdateTaskNameAsync), nameof(AccessorClient));

        try
        {
            var req = _daprClient.CreateInvokeMethodRequest(HttpMethod.Patch, AppIds.Accessor, "tasks-accessor/task");
            req.Headers.IfMatch.Clear();
            if (!string.IsNullOrWhiteSpace(ifMatch))
            {
                var tag = ifMatch.Trim().Trim('"');
                req.Headers.IfMatch.Add(new EntityTagHeaderValue($"\"{tag}\""));
            }

            var body = new { id, name = newTaskName };
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var resp = await _daprClient.InvokeMethodWithResponseAsync(req, ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Task {TaskId} not found for update", id);
                return new UpdateTaskNameResult(false, true, false, null);
            }

            if ((int)resp.StatusCode == StatusCodes.Status412PreconditionFailed)
            {
                _logger.LogWarning("Precondition failed for Task {TaskId} (ETag mismatch)", id);
                return new UpdateTaskNameResult(false, false, true, null);
            }

            resp.EnsureSuccessStatusCode();

            var newEtag = resp.Headers.ETag?.Tag?.Trim('"');
            _logger.LogInformation("Task {TaskId} updated; new ETag {ETag}", id, newEtag);
            return new UpdateTaskNameResult(true, false, false, newEtag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to PATCH update task {TaskId} at Accessor", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<WordCard>> GetWordCardsAsync(Guid userId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching word cards for user {UserId}", userId);

        try
        {
            var wordCards = await _daprClient.InvokeMethodAsync<List<WordCard>>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"wordcards-accessor/{userId}",
                cancellationToken: ct
            );

            return wordCards ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch word cards for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WordCard> CreateWordCardAsync(Guid userId, CreateWordCardRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating word card for user {UserId}", userId);

        try
        {
            var payload = new CreateWordCard
            {
                UserId = userId,
                Hebrew = request.Hebrew,
                English = request.English,
                Explanation = request.Explanation,
            };

            var response = await _daprClient.InvokeMethodAsync<CreateWordCard, WordCard>(
                HttpMethod.Post,
                AppIds.Accessor,
                $"wordcards-accessor",
                payload,
                cancellationToken: ct
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create word card for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WordCardLearnedStatus> UpdateLearnedStatusAsync(Guid userId, LearnedStatus request, CancellationToken ct)
    {
        _logger.LogInformation("Updating learned status. UserId={UserId}, CardId={CardId}, IsLearned={IsLearned}", userId, request.CardId, request.IsLearned);

        try
        {
            var payload = new SetLearnedStatus
            {
                UserId = userId,
                CardId = request.CardId,
                IsLearned = request.IsLearned,
            };

            var response = await _daprClient.InvokeMethodAsync<SetLearnedStatus, WordCardLearnedStatus>(
                HttpMethod.Patch,
                AppIds.Accessor,
                $"wordcards-accessor/learned",
                payload,
                cancellationToken: ct
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update learned status for CardId={CardId}", request.CardId);
            throw;
        }
    }

    public async Task<UserGameConfig> GetUserGameConfigAsync(Guid userId, GameName gameName, CancellationToken ct)
    {
        _logger.LogInformation("Get User's Game Configuration. UserId={UserId}, Game Name={GameName}", userId, gameName);

        try
        {

            var response = await _daprClient.InvokeMethodAsync<UserGameConfig>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"game-config-accessor?userId={userId}&gameName={gameName}",
                cancellationToken: ct
            );
            return response;
        }
        catch (Exception)
        {
            _logger.LogInformation("Failed to get user configuration for UserId={UserId}", userId);
            throw;
        }
    }

    public async Task SaveUserGameConfigAsync(Guid userId, UserNewGameConfig gameName, CancellationToken ct)
    {
        _logger.LogInformation("Save User's Game Configuration. UserId={UserId}, Game Name={GameName}", userId, gameName);
        try
        {
            var payload = _mapper.Map<UserGameConfig>(gameName);
            payload.UserId = userId;

            await _daprClient.InvokeMethodAsync(
                HttpMethod.Put,
                AppIds.Accessor,
                $"game-config-accessor",
                payload,
                cancellationToken: ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user configuration for UserId={UserId}", userId);
            throw;
        }
    }

    public async Task DeleteUserGameConfigAsync(Guid userId, GameName gameName, CancellationToken ct)
    {
        _logger.LogInformation("Delete User's Game Configuration. UserId={UserId}, Game Name={GameName}", userId, gameName);
        try
        {
            var payload = new UserGameConfigKey
            {
                UserId = userId,
                GameName = gameName
            };

            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                AppIds.Accessor,
                $"game-config-accessor",
                payload,
                cancellationToken: ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user configuration for UserId={UserId}", userId);
            throw;
        }
    }

    public async Task UpdateUserLanguageAsync(Guid callerId, SupportedLanguage language, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating user language to {Language}", language);

        var payload = new UserLanguage
        {
            UserId = callerId,
            Language = language
        };

        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Put,
                AppIds.Accessor,
                $"users-accessor/language",
                payload,
                cancellationToken: ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user language to {Language}", language);
            throw;
        }
    }
}