using System.Text.Json;
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

    public async Task SendNotificationAsync(string userId, UserNotification notification)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }

        await _hubContext.Clients.User(userId).NotificationMessage(notification);
    }

    public async Task SendEventAsync<TPayload>(EventType eventType, string userId, TPayload payload)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            // now frontend that listen to "ReceiveEvent" method in signal r will receive this event and can see in event type what the payload contains
            var jsonPayload = JsonSerializer.SerializeToElement(payload);
            var evt = new UserEvent<JsonElement>
            {
                eventType = eventType, // Type of the event, e.g., "ChatMessage", "UserJoined", etc.
                Payload = jsonPayload // The actual data being sent with the event (e.g., chat message class, user info class, etc.)
            };

            await _hubContext.Clients.User(userId).ReceiveEvent(evt);
            _logger.LogInformation("Event '{EventType}' sent successfully", eventType);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Invalid JSON payload for event '{EventType}'", eventType);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending event '{EventType}'", eventType);
            throw;
        }
    }
}
