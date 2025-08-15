using Manager.Models;
// here we can add methods for different channels of signalR - so we can split to notifications events, etc.
namespace Manager.Hubs;

public interface INotificationClient
{
    // for example, frontend listen to "NotificationMessage" method in signal r to receive Notification
    Task NotificationMessage(UserNotification notification);
    // frontend listen to "ReceiveEvent" method in signal r to receive Event 
    Task ReceiveEvent<TPayload>(UserEvent<TPayload> evt);
}