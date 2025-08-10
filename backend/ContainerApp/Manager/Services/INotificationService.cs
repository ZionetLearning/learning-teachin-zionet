namespace Manager.Services;

public interface INotificationService
{
    Task SendEventAsync<TPayload>(string eventType, TPayload payload);

    Task SendNotificationAsync(string userId, string message);
}
