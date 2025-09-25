using System.Text.Json;
using Manager.Hubs;
using Manager.Models.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace Manager.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    private static readonly JsonSerializerOptions s_payloadJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
            var jsonPayload = JsonSerializer.SerializeToElement(payload, s_payloadJsonOptions);

            var evt = new UserEvent<JsonElement>
            {
                EventType = eventType,
                Payload = jsonPayload
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

    public async Task SendStreamEventAsync<T>(StreamEvent<T> streamEvent, string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }

        try
        {
            var jsonPayload = JsonSerializer.SerializeToElement(streamEvent.Payload, s_payloadJsonOptions);

            var jsonStreamEvent = new StreamEvent<JsonElement>
            {
                EventType = streamEvent.EventType,
                Payload = jsonPayload,
                SequenceNumber = streamEvent.SequenceNumber,
                Stage = streamEvent.Stage,
                RequestId = streamEvent.RequestId
            };

            await _hubContext.Clients.User(userId).StreamEvent(jsonStreamEvent);
            _logger.LogInformation("Stream event '{EventType}' sent successfully", streamEvent.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending stream event '{EventType}'", streamEvent.EventType);
            throw;
        }
    }
}
