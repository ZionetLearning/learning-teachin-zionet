using System.Net;
using System.Text.Json;
using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using Manager.Models.Auth;
using Manager.Models.Auth.RefreshSessions;
using Manager.Models.Chat;
using Manager.Models.QueueMessages;
using Manager.Models.Users;

namespace Manager.Services.Clients.Accessor;

public class AccessorClient(
    ILogger<AccessorClient> logger,
    DaprClient daprClient,
    IHttpContextAccessor httpContextAccessor
    ) : IAccessorClient
{
    private readonly ILogger<AccessorClient> _logger = logger;
    private readonly DaprClient _daprClient = daprClient;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<TaskModel?> GetTaskAsync(int id)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetTaskAsync), nameof(AccessorClient));
        try
        {
            var task = await _daprClient.InvokeMethodAsync<TaskModel?>(
                HttpMethod.Get, "accessor", $"tasks-accessor/task/{id}");
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
            await _daprClient.InvokeMethodAsync(HttpMethod.Delete, "accessor", $"tasks-accessor/task/{id}");
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
            var userId = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault() ?? "anonymous";

            var payload = JsonSerializer.SerializeToElement(task);
            var userContextMetadata = JsonSerializer.SerializeToElement(
                new UserContextMetadata
                {
                    UserId = userId,
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
                "Task {TaskId} sent to Accessor via binding '{Binding}'",
                task.Id,
                QueueNames.AccessorQueue
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
                HttpMethod.Get, "accessor", $"chats-accessor/{userId}", cancellationToken: ct);

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

    public async Task<UserData?> GetUserAsync(Guid userId)
    {
        try
        {
            return await _daprClient.InvokeMethodAsync<UserData?>(
                HttpMethod.Get, "accessor", $"users-accessor/{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> CreateUserAsync(UserModel user)
    {
        try
        {
            _logger.LogInformation("Creating user with email: {Email}", user.Email);

            await _daprClient.InvokeMethodAsync(HttpMethod.Post, "accessor", "users-accessor", user);

            _logger.LogInformation("User {Email} created successfully", user.Email);
            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning("Conflict: User already exists: {Email}", user.Email);
            return false;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Bad request when creating user: {Email}", user.Email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", user.Email);
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(UpdateUserModel user, Guid userId)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(HttpMethod.Put, "accessor", $"users-accessor/{userId}", user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(HttpMethod.Delete, "accessor", $"users-accessor/{userId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<UserData>> GetAllUsersAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetAllUsersAsync), nameof(AccessorClient));

        try
        {
            var users = await _daprClient.InvokeMethodAsync<List<UserData>>(
                HttpMethod.Get, "accessor", "users-accessor", ct);

            _logger.LogInformation("Retrieved {Count} users from accessor", users?.Count ?? 0);
            return users ?? Enumerable.Empty<UserData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all users from accessor");
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
}