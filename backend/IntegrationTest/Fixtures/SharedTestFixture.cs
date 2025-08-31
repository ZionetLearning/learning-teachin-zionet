using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Constants;
using IntegrationTests.Infrastructure;
using Manager.Models.Auth; // for LoginRequest / AuthRoutes if defined there
using Xunit.Abstractions;

namespace IntegrationTests.Fixtures;

public class SharedTestFixture : IAsyncLifetime
{
    public HttpTestFixture HttpFixture { get; } = new();
    public TestUserFixture UserFixture { get; }

    private string? _accessToken;
    private readonly SemaphoreSlim _authLock = new(1, 1);

    public SharedTestFixture()
    {
        UserFixture = new TestUserFixture(HttpFixture);
    }

    public async Task InitializeAsync() => await UserFixture.InitializeAsync();

    public async Task DisposeAsync()
    {
        await UserFixture.DisposeAsync();
        HttpFixture.Dispose();
        _authLock.Dispose();
    }

    /// <summary>
    /// Logs in the seeded test user on demand, caches the access token,
    /// and (by default) attaches Authorization to the shared HttpClient.
    /// </summary>
    public async Task<string> GetAuthenticatedTokenAsync(
        bool attachToHttpClient = true,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(_accessToken))
        {
            if (attachToHttpClient)
            {
                HttpFixture.Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
            }
            return _accessToken!;
        }

        await _authLock.WaitAsync(ct);
        try
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                var user = UserFixture.TestUser;
                var res = await HttpFixture.Client.PostAsJsonAsync(
                    AuthRoutes.Login, // e.g. "/auth/login"
                    new LoginRequest { Email = user.Email, Password = user.Password },
                    ct
                );

                res.EnsureSuccessStatusCode();

                var body = await res.Content.ReadAsStringAsync(ct);
                var dto = JsonSerializer.Deserialize<IntegrationTests.Models.Auth.AccessTokenResponse>(body)
                          ?? throw new InvalidOperationException("Invalid JSON for access token.");
                if (string.IsNullOrWhiteSpace(dto.AccessToken))
                    throw new InvalidOperationException("No access token returned from login.");

                _accessToken = dto.AccessToken;
            }
        }
        finally
        {
            _authLock.Release();
        }

        if (attachToHttpClient)
        {
            HttpFixture.Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        return _accessToken!;
    }

    /// <summary>
    /// Starts SignalR once with the authenticated token. Logs & rethrows on failure.
    /// </summary>
    public async Task EnsureSignalRStartedAsync(
        SignalRTestFixture signalR,
        ITestOutputHelper? output = null,
        CancellationToken ct = default)
    {
        var token = await GetAuthenticatedTokenAsync(attachToHttpClient: true, ct);
        signalR.UseAccessToken(token);

        try
        {
            await signalR.StartAsync();
        }
        catch (Exception ex)
        {
            output?.WriteLine($"❌ Failed to start SignalR connection: {ex}");
            throw; // bubble up
        }
    }
}
