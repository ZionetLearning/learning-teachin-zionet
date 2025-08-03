using Microsoft.AspNetCore.SignalR;
using Manager.Models;

namespace Manager.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        this._logger = logger;
    }

    public async Task SendTaskUpdate(int taskId, string status)
    {
        try
        {
            this._logger.LogInformation("Sending task update for TaskId: {TaskId}, Status: {Status}", taskId, status);
            await this.Clients.All.SendAsync("TaskUpdated", new TaskUpdateMessage { TaskId = taskId, Status = status });
            this._logger.LogInformation("Task update sent successfully for TaskId: {TaskId}", taskId);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to send task update for TaskId: {TaskId}, Status: {Status}", taskId, status);
        }
    }

    public async Task SendNotification(string message)
    {
        try
        {
            this._logger.LogInformation("Sending notification with message: {Message}", message);
            await this.Clients.All.SendAsync("NotificationReceived", new NotificationMessage { Message = message });
            this._logger.LogInformation("Notification sent successfully");
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to send notification with message: {Message}", message);
        }
    }
}
