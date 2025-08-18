using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Models.Notification;
using Xunit.Abstractions;

namespace IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
    : IClassFixture<HttpTestFixture>,
        IClassFixture<SignalRTestFixture>,
        IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly SignalRTestFixture SignalRFixture;
    protected readonly ITestOutputHelper OutputHelper;

    protected IntegrationTestBase(
        HttpTestFixture httpFixture,
        ITestOutputHelper testOutputHelper,
        SignalRTestFixture signalRFixture
    )
    {
        Client = httpFixture.Client;
        OutputHelper = testOutputHelper;
        SignalRFixture = signalRFixture;
    }

    protected IntegrationTestBase(
        AccessorHttpTestFixture fixture,
        ITestOutputHelper testOutputHelper,
        SignalRTestFixture signalRFixture
    )
    {
        OutputHelper = testOutputHelper;
        Client = fixture.Client;
        SignalRFixture = signalRFixture;
    }

    public virtual async Task InitializeAsync()
    {
        OutputHelper.WriteLine("Starting SignalR connection...");
        await SignalRFixture.StartAsync();
        SignalRFixture.ClearReceivedMessages();
        OutputHelper.WriteLine("SignalR connection ready.");
    }

    public virtual async Task DisposeAsync()
    {
        OutputHelper.WriteLine("Stopping SignalR connection...");
        await SignalRFixture.StopAsync();
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        return await Client.PostAsJsonAsync(requestUri, value);
    }

    protected async Task<T?> ReadAsJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed with status {response.StatusCode}: {content}");
        }

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Empty response content");

        return JsonSerializer.Deserialize<T>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }

    protected async Task<ReceivedNotification> WaitForNotificationAsync(
        Predicate<UserNotification>? predicate = null,
        TimeSpan? timeout = null
    )
    {
        var notification = await SignalRFixture.WaitForNotificationAsync(predicate, timeout);
        notification.Should().NotBeNull("Expected a SignalR notification");
        return notification!;
    }

    protected async Task<ReceivedEvent> WaitForEventAsync(
        Predicate<UserEvent<JsonElement>>? predicate = null,
        TimeSpan? timeout = null
    )
    {
        var evt = await SignalRFixture.WaitForEventAsync(predicate, timeout);
        evt.Should().NotBeNull("Expected a SignalR event");
        return evt!;
    }
}
