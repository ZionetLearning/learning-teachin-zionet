using System.Net;
using System.Text.Json;
using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using Manager.Models.Chat;
using Manager.Models.QueueMessages;
using Manager.Models.Users;

namespace Manager.Services.Clients;

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
                HttpMethod.Get,
                "accessor",
                $"task/{id}"
            );
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
                id = id,
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
            await _daprClient.InvokeMethodAsync(HttpMethod.Delete, "accessor", $"task/{id}");
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

    public async Task<IReadOnlyList<ChatMessage>> GetChatHistoryAsync(Guid threadId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetChatHistoryAsync), nameof(AccessorClient));
        try
        {
            var messages = await _daprClient.InvokeMethodAsync<List<ChatMessage>>(
                HttpMethod.Get,
                "accessor",
                $"threads/{threadId}/messages",
                cancellationToken: ct
            );

            return messages ?? new List<ChatMessage>();
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

    public async Task<ChatMessage?> StoreMessageAsync(ChatMessage msg, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(StoreMessageAsync), nameof(AccessorClient));
        try
        {
            var created = await _daprClient.InvokeMethodAsync<ChatMessage, ChatMessage>(
                HttpMethod.Post,
                "accessor",
                "threads/message",
                msg,
                cancellationToken: ct
            );

            _logger.LogDebug("Message stored in thread {ThreadId}", msg.ThreadId);
            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store message for thread {ThreadId}", msg.ThreadId);
            throw;
        }
    }

    public async Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(string userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetThreadsForUserAsync), nameof(AccessorClient));

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("userId cannot be null or whitespace.", nameof(userId));
        }

        try
        {
            var threads = await _daprClient.InvokeMethodAsync<List<ChatThread>>(
                HttpMethod.Get,
                "accessor",
                $"threads/{userId}",
                cancellationToken: ct
            );

            return threads ?? new List<ChatThread>();
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No threads found for user {UserId}", userId);
            return Array.Empty<ChatThread>();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("{Metod} cancelled for user {UserId}", nameof(GetThreadsForUserAsync), userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get threads for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserModel?> GetUserAsync(Guid userId)
    {
        try
        {
            return await _daprClient.InvokeMethodAsync<UserModel?>(
                HttpMethod.Get,
                "accessor",
                $"users/{userId}");
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
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Post,
                "accessor",
                "users",
                user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", user.Email);
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync(UpdateUserModel user, Guid userId)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Put,
                "accessor",
                $"users/{userId}",
                user);
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
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                "accessor",
                $"users/{userId}");
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
                HttpMethod.Get,
                "accessor",
                "users",
                ct
            );

            _logger.LogInformation("Retrieved {Count} users from accessor", users?.Count ?? 0);
            return users ?? Enumerable.Empty<UserData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all users from accessor");
            throw;
        }
    }
}