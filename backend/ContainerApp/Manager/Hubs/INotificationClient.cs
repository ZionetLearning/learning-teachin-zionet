using System.Text.Json;
using Manager.Models.Notifications;
// here we can add methods for different channels of signalR - so we can split to notifications events, etc.
namespace Manager.Hubs;

public interface INotificationClient
{
    // for example, frontend listen to "NotificationMessage" method in signal r to receive Notification
    Task NotificationMessage(UserNotification notification);
    // frontend listen to "ReceiveEvent" method in signal r to receive Event 
    Task ReceiveEvent(UserEvent<JsonElement> evt);

    Task StreamEvent(StreamEvent<JsonElement> evt);

    Task UserOnline(string userId, string role, string name);
    Task UserOffline(string userId);
    Task UpdateUserConnections(string userId, int count);
}