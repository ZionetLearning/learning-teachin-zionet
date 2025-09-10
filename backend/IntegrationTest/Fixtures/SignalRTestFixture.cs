using IntegrationTests.Constants;
using IntegrationTests.Models.Notification;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntegrationTests.Fixtures;

public class SignalRTestFixture : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly ConcurrentQueue<ReceivedNotification> _receivedNotifications = new();
    private readonly ConcurrentQueue<ReceivedEvent> _receivedEvents = new();

    private readonly string _baseUrl;
    private string? _accessToken;

    public SignalRTestFixture()
    {
        _baseUrl = GetBaseUrl();
    }

    public void UseAccessToken(string token) => _accessToken = token;

    private void SetupEventHandlers()
    {
        _connection!.On<UserNotification>("NotificationMessage", notification =>
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
        if (_connection is null)
        {

            _connection = new HubConnectionBuilder()
                .WithUrl($"{_baseUrl}/notificationHub", options =>
                {
                    if (!string.IsNullOrEmpty(_accessToken))
                        options.AccessTokenProvider = () => Task.FromResult(_accessToken)!;
                })
                .AddJsonProtocol(o =>
                {
                    o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    o.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                })
                .WithAutomaticReconnect()
                .Build();

            SetupEventHandlers();
        }

        if (_connection.State != HubConnectionState.Connected)
        {
            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start SignalR connection: {ex.Message}");
                throw;
            }
        }
    }


    public async Task StopAsync()
    {
        if (_connection is not null && _connection.State == HubConnectionState.Connected)
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
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var url = config["TestSettings:ApiBaseUrl"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException(
                "ApiBaseUrl is not set in appsettings.json under TestSettings."
            );
        }

        return url.TrimEnd('/');
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
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