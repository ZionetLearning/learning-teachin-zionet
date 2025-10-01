using Manager.Models.Notifications;

namespace Manager.Services;

public interface INotificationService
{
    Task SendEventAsync<TPayload>(EventType eventType, string userId, TPayload payload);
    Task SendNotificationAsync(string userId, UserNotification notification);
    Task SendStreamEventAsync<TPayload>(StreamEvent<TPayload> streamEvent, string userId);
}
