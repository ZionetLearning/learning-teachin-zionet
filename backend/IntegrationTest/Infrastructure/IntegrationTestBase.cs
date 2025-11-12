using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using IntegrationTests.Fixtures;
using IntegrationTests.Models.Notification;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
    : IClassFixture<HttpClientFixture>,
        IClassFixture<SignalRTestFixture>,
        IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly SignalRTestFixture SignalRFixture;
    protected readonly HttpClientFixture ClientFixture;
    protected readonly ITestOutputHelper OutputHelper;
    private static readonly JsonSerializerOptions CachedJsonOptions = new() 
    { 
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    public IConfiguration Configuration { get; }


    protected IntegrationTestBase(
        HttpClientFixture httpClientFixture,
        ITestOutputHelper testOutputHelper,
        SignalRTestFixture signalRFixture
    )
    {
        Client = httpClientFixture.Client;
        OutputHelper = testOutputHelper;
        SignalRFixture = signalRFixture;
        ClientFixture = httpClientFixture;
        try
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
        catch (Exception ex)
        {
            // Fail fast if config cannot be built
            throw new InvalidOperationException("Failed to build test configuration.", ex);
        }
    }

    // Default: Do NOT auto-start SignalR. Subclasses that need it (after auth) should start explicitly.
    public virtual Task InitializeAsync()
    {
        SignalRFixture.ClearReceivedMessages();
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        OutputHelper.WriteLine("Stopping SignalR connection (if active)...");
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
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default;
            }

            // For non-nullable value types, empty content is likely an error
            var type = typeof(T);
            var isNullable = !type.IsValueType || Nullable.GetUnderlyingType(type) != null;

            if (!isNullable)
            {
                throw new InvalidOperationException(
                    $"Cannot convert empty response content to non-nullable type {typeof(T).Name}. " +
                    $"Expected JSON content but received empty response with status {response.StatusCode}."
                );
            }

            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(content, CachedJsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize response as {typeof(T).Name}: {ex.Message}", ex);
        }
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

    protected async Task<ReceivedNotification?> TryWaitForNotificationAsync(
    Predicate<UserNotification> predicate,
    TimeSpan? timeout = null)
    {
        try
        {
            return await WaitForNotificationAsync(predicate, timeout);
        }
        catch
        {
            return null; // swallow timeout, return null
        }
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

    /// <summary>
    /// Starts the SignalR connection using the current authentication token from the HttpClient.
    /// If no token is present, starts without authentication.
    /// </summary>
    protected async Task EnsureSignalRStartedAsync()
    {
        var token = Client.DefaultRequestHeaders.Authorization?.Parameter;
        
        if (!string.IsNullOrEmpty(token))
        {
            SignalRFixture.UseAccessToken(token);
        }

        try
        {
            await SignalRFixture.StartAsync();
            OutputHelper.WriteLine("SignalR connection started successfully.");
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine($"Failed to start SignalR connection: {ex.Message}");
            throw;
        }
    }
}
