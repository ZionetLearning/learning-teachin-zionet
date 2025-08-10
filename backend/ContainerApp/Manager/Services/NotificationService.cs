
using Manager.Hubs;
using Manager.Models;
using Microsoft.AspNetCore.SignalR;

namespace Manager.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
    IHubContext<NotificationHub, INotificationClient> hubContext,
    ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationAsync(string userId, string message)
    {
        await _hubContext.Clients.User(userId).NotificationMessage(userId, message);
    }

    public async Task SendEventAsync<TPayload>(string eventType, TPayload payload)
    {
        try
        {
            // now frontend that listen to "ReceiveEvent" method in signal r will receive this event and can see in event type what the paylod is of
            var evt = new Event<TPayload>
            {
                Type = eventType, // Type of the event, e.g., "ChatMessage", "UserJoined", etc.
                Payload = payload // The actual data being sent with the event (e.g., chat message class, user info class, etc.)
            };

            await _hubContext.Clients.All.ReceiveEvent(evt);
            _logger.LogInformation("Event '{EventType}' sent successfully", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending event '{EventType}'", eventType);
            throw;
        }

        /* example of usage:
         * 
        public class ChatManager
        {
            private readonly INotificationService _notifications;

            public ChatManager(INotificationService notifications)
            {
                _notifications = notifications;
            }

            public async Task SendChatMessageAsync(string sender, string message)
            {
                await _notifications.SendEventAsync("chatMessage", new ChatMessagePayload
                {
                    Sender = sender,
                    Message = message
                });
            }
        
        }
        */
    }
}
