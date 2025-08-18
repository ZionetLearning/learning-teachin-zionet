using Microsoft.AspNetCore.SignalR.Client;
using DotNetEnv;
using System.Collections.Concurrent;
using System.Text.Json;
using IntegrationTests.Models.Notification;
using IntegrationTests.Constants;

namespace IntegrationTests.Infrastructure;

public class SignalRTestFixture : IDisposable
{
    private readonly HubConnection _connection;
    private readonly ConcurrentQueue<ReceivedNotification> _receivedNotifications = new();
    private readonly ConcurrentQueue<ReceivedEvent> _receivedEvents = new();

    public SignalRTestFixture()
    {
        Env.Load();
        var baseUrl = GetBaseUrl();
        
        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/notificationHub?userId={TestConstants.TestUserId}")
            .Build();

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _connection.On<UserNotification>("NotificationMessage", notification =>
        {
            _receivedNotifications.Enqueue(new ReceivedNotification
            {
                Notification = notification,
                ReceivedAt = DateTimeOffset.UtcNow
            });
        });

        _connection.On<UserEvent<JsonElement>>("ReceiveEvent", evt =>
        {
            _receivedEvents.Enqueue(new ReceivedEvent
            {
                Event = evt,
                ReceivedAt = DateTimeOffset.UtcNow
            });
        });
    }

    public async Task StartAsync()
    {
        if (_connection.State != HubConnectionState.Connected)
        {
            await _connection.StartAsync();
        }
    }

    public async Task StopAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
        {
            await _connection.StopAsync();
        }
    }

    public IReadOnlyList<ReceivedNotification> GetReceivedNotifications()
    {
        return _receivedNotifications.ToList();
    }

    public IReadOnlyList<ReceivedEvent> GetReceivedEvents()
    {
        return _receivedEvents.ToList();
    }

    public void ClearReceivedMessages()
    {
        _receivedNotifications.Clear();
        _receivedEvents.Clear();
    }

    public async Task<ReceivedNotification?> WaitForNotificationAsync(
        Predicate<UserNotification>? predicate = null, 
        TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var endTime = DateTime.UtcNow.Add(timeout.Value);

        while (DateTime.UtcNow < endTime)
        {
            var notification = _receivedNotifications
                .FirstOrDefault(n => predicate?.Invoke(n.Notification) ?? true);
            
            if (notification != null)
                return notification;

            await Task.Delay(100);
        }

        return null;
    }

    public async Task<ReceivedEvent?> WaitForEventAsync(
        Predicate<UserEvent<JsonElement>>? predicate = null,
        TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var endTime = DateTime.UtcNow.Add(timeout.Value);

        while (DateTime.UtcNow < endTime)
        {
            var evt = _receivedEvents
                .FirstOrDefault(e => predicate?.Invoke(e.Event) ?? true);
            
            if (evt != null)
                return evt;

            await Task.Delay(100);
        }

        return null;
    }

    private static string GetBaseUrl()
    {
        var url = Environment.GetEnvironmentVariable("API_BASE_URL");
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException(
                "API_BASE_URL is not set. Please define it in the .env file or environment variables."
            );
        }
        return url.TrimEnd('/');
    }

    public void Dispose()
    {
        _connection?.DisposeAsync().AsTask().Wait();
        GC.SuppressFinalize(this);
    }
}

public record ReceivedNotification
{
    public required UserNotification Notification { get; init; }
    public DateTimeOffset ReceivedAt { get; init; }
}

public record ReceivedEvent
{
    public required UserEvent<JsonElement> Event { get; init; }
    public DateTimeOffset ReceivedAt { get; init; }
}