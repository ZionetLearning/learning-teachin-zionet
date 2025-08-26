using AutoMapper;
using Manager.Models;
using Manager.Services.Clients;
using Manager.Services.Clients.Engine;
using Manager.Models.Users;
using System.Text.Json;
using Manager.Models.Notifications;

namespace Manager.Services;

public class ManagerService : IManagerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ManagerService> _logger;
    private readonly IAccessorClient _accessorClient;
    private readonly IEngineClient _engineClient;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public ManagerService(IConfiguration configuration,
        ILogger<ManagerService> logger,
        IAccessorClient accessorClient,
        IEngineClient engineClient,
        IMapper mapper,
        INotificationService notificationService)
    {
        _configuration = configuration;
        _logger = logger;
        _accessorClient = accessorClient;
        _engineClient = engineClient;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<TaskModel?> GetTaskAsync(int id)
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(GetTaskAsync));

        if (id <= 0)
        {
            _logger.LogWarning("Invalid task ID provided: {TaskId}", id);
            return null;
        }

        try
        {
            var task = await _accessorClient.GetTaskAsync(id);
            if (task != null)
            {
                _logger.LogDebug("Successfully retrieved task {TaskId}", id);
            }
            else
            {
                _logger.LogDebug("Task {TaskId} not found", id);
            }

            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting task {TaskId}", id);
            throw;
        }
    }

    public async Task<(bool success, string message)> CreateTaskAsync(TaskModel task)
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(CreateTaskAsync));

        if (task is null)
        {
            _logger.LogWarning("Null task received for processing");
            return (false, "Task is null");
        }

        if (string.IsNullOrWhiteSpace(task.Name))
        {
            _logger.LogWarning("Task {TaskId} has invalid name", task.Id);
            return (false, "Task name is required");
        }

        if (string.IsNullOrWhiteSpace(task.Payload))
        {
            _logger.LogWarning("Task {TaskId} has invalid payload", task.Id);
            return (false, "Task payload is required");
        }

        try
        {
            _logger.LogDebug("Posting task {TaskId} with name '{TaskName}'", task.Id, task.Name);
            var result = await _accessorClient.PostTaskAsync(task);
            if (result.success)
            {
                _logger.LogDebug("Task {TaskId} successfully posted to queue", task.Id);
            }
            else
            {
                _logger.LogDebug("Task {TaskId} processing failed: {Message}", task.Id, result.message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing task {TaskId}", task.Id);
            return (false, "Failed to send to Engine");
        }
    }

    public async Task<(bool success, string message)> ProcessTaskLongAsync(TaskModel task)
    {
        try
        {
            _logger.LogDebug("Inside: {MethodName}", nameof(CreateTaskAsync));
            var result = await _engineClient.ProcessTaskLongAsync(task);
            return (result.success, result.message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing task {TaskId}", task.Id);
            return (false, "Failed to send to Engine");
        }
    }

    public async Task<bool> UpdateTaskName(int id, string newTaskName)
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(UpdateTaskName));

        if (id <= 0)
        {
            _logger.LogWarning("Invalid task ID provided for update: {TaskId}", id);
            return false;
        }

        if (string.IsNullOrWhiteSpace(newTaskName))
        {
            _logger.LogWarning("Invalid task name provided for update: '{TaskName}'", newTaskName);
            return false;
        }

        if (newTaskName.Length > 100)
        {
            _logger.LogWarning("Task name too long for task {TaskId}: {Length} characters", id, newTaskName.Length);
            return false;
        }

        try
        {
            _logger.LogInformation("Updating task {TaskId} name to '{NewTaskName}'", id, newTaskName);
            var result = await _accessorClient.UpdateTaskName(id, newTaskName);
            if (result)
            {
                _logger.LogInformation("Task {TaskId} name successfully updated", id);
            }
            else
            {
                _logger.LogWarning("Failed to update task {TaskId} name", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating task {TaskId} name", id);
            return false;
        }
    }

    public async Task<bool> DeleteTask(int id)
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(DeleteTask));
        if (id <= 0)
        {
            _logger.LogWarning("Invalid task ID provided for deletion: {TaskId}", id);
            return false;
        }

        try
        {
            _logger.LogInformation("Attempting to delete task {TaskId}", id);
            var result = await _accessorClient.DeleteTask(id);
            if (result)
            {
                _logger.LogInformation("Task {TaskId} successfully deleted", id);
            }
            else
            {
                _logger.LogWarning("Failed to delete task {TaskId}", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting task {TaskId}", id);
            return false;
        }
    }

    public async Task SendUserNotificationAsync(string userId, UserNotification notification)
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(SendUserNotificationAsync));

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Invalid user ID provided for notification");
            throw new ArgumentException("User ID is required", nameof(userId));
        }

        if (notification is null)
        {
            _logger.LogWarning("Null notification received for user {UserId}", userId);
            throw new ArgumentNullException(nameof(notification));
        }

        try
        {
            _logger.LogInformation("Sending notification to user {UserId} with message: {Message}", userId, notification.Message);
            await _notificationService.SendNotificationAsync(userId, notification);
            _logger.LogInformation("Notification sent successfully to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending notification to user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserModel?> GetUserAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Fetching user with ID: {UserId}", userId);
            return await _accessorClient.GetUserAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user with ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> CreateUserAsync(UserModel user)
    {
        try
        {
            _logger.LogInformation("Creating user with email: {Email}", user.Email);
            return await _accessorClient.CreateUserAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user with email: {Email}", user.Email);
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync(UpdateUserModel user, Guid userId)
    {
        try
        {
            _logger.LogInformation("Updating user with ID: {UserId}", userId);
            return await _accessorClient.UpdateUserAsync(user, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with ID: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", userId);
            return await _accessorClient.DeleteUserAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user with ID: {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<UserData>> GetAllUsersAsync()
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(GetAllUsersAsync));

        try
        {
            var users = await _accessorClient.GetAllUsersAsync();

            if (users is null || !users.Any())
            {
                _logger.LogWarning("No users found in the system");
                return [];
            }

            _logger.LogInformation("Retrieved {Count} users", users.Count());
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all users");
            throw;
        }
    }

    public async Task SendUserEventAsync<T>(string userId, UserEvent<T> userEvent)
    {
        _logger.LogDebug("Inside: {MethodName}", nameof(SendUserEventAsync));

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Invalid user ID provided for event");
            throw new ArgumentException("User ID is required", nameof(userId));
        }

        if (userEvent is null)
        {
            _logger.LogWarning("Null user event received for user {UserId}", userId);
            throw new ArgumentNullException(nameof(userEvent));
        }

        if (userEvent.Payload is null && typeof(T).IsClass)
        {
            _logger.LogWarning("Null payload for event {EventType} to user {UserId}", userEvent.EventType, userId);
        }

        try
        {
            _logger.LogInformation("Sending event {EventType} to user {UserId}", userEvent.EventType, userId);
            await _notificationService.SendEventAsync(userEvent.EventType, userId, userEvent.Payload);
            _logger.LogInformation("Event {EventType} sent successfully to user {UserId}", userEvent.EventType, userId);
        }
        catch (JsonException jex)
        {
            _logger.LogError(jex, "JSON error occurred while sending event {EventType} to user {UserId}", userEvent.EventType, userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending event {EventType} to user {UserId}", userEvent.EventType, userId);
            throw;
        }
    }
}
