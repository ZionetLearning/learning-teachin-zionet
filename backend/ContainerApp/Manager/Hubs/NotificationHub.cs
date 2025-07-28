using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Manager.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public async Task SendTaskUpdate(int taskId, string status)
    {
        try
        {
            _logger.LogInformation("Sending task update for TaskId: {TaskId}, Status: {Status}", taskId, status);
            await Clients.All.SendAsync("TaskUpdated", taskId, status);
            _logger.LogInformation("Task update sent successfully for TaskId: {TaskId}", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task update for TaskId: {TaskId}, Status: {Status}", taskId, status);
        }
    }

    public async Task SendNotification(string message)
    {
        try
        {
            _logger.LogInformation("Sending notification with message: {Message}", message);
            await Clients.All.SendAsync("NotificationReceived", message);
            _logger.LogInformation("Notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification with message: {Message}", message);
        }
    }
}

