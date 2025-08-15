using Manager.Models;

namespace Manager.Services;

public interface INotificationService
{
    Task SendEventAsync<TPayload>(EventType eventType, string userId, TPayload payload);
    Task SendNotificationAsync(string userId, UserNotification notification);
}
