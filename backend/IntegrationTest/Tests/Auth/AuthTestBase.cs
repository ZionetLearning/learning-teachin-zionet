using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models.Auth;
using Manager.Constants;
using Manager.Models.Auth;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Auth;

public abstract class AuthTestBase : IntegrationTestBase
{
    protected AuthTestBase(HttpTestFixture fixture, ITestOutputHelper outputHelper, SignalRTestFixture signalRFixture)
        : base(fixture, outputHelper, signalRFixture)
    {
    }

    // Override the base Initialize so we DO NOT start SignalR before login (Auth tests validate login/refresh/logout flows)
    public override Task InitializeAsync()
    {
        // Just clear any stale in-memory messages; connection not started yet because we lack an access token.
        SignalRFixture.ClearReceivedMessages();
        return Task.CompletedTask;
    }

    protected async Task<HttpResponseMessage> LoginAsync(string email, string password)
    {
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };
        return await Client.PostAsJsonAsync(AuthRoutes.Login, loginRequest);
    }

    protected async Task<HttpResponseMessage> LogoutAsync(string refreshToken)
    {
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, AuthRoutes.Logout);
        logoutRequest.Headers.Add("Cookie", $"{AuthSettings.RefreshTokenCookieName}={refreshToken}");
        return await Client.SendAsync(logoutRequest);
    }

    protected async Task<string> ExtractAccessToken(HttpResponseMessage response, CancellationToken ct = default)
    {
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<AccessTokenResponse>(body)
                     ?? throw new InvalidOperationException("Invalid JSON response");

        return result.AccessToken;
    }

    protected static (string? RefreshToken, string? CsrfToken) ExtractTokens(HttpResponseMessage response)
    {
        return (
            CookieHelper.ExtractCookieFromHeaders(response, TestConstants.RefreshToken),
            CookieHelper.ExtractCookieFromHeaders(response, TestConstants.CsrfToken)
        );
    }

    protected HttpRequestMessage CreateRefreshRequest(string refreshToken, string csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, AuthRoutes.Refresh);
        request.Headers.Add("X-CSRF-Token", csrfToken);
        request.Headers.Add("Cookie", $"{TestConstants.RefreshToken}={refreshToken}; {TestConstants.CsrfToken}={csrfToken}");
        return request;
    }

    // Helper for tests that DO need SignalR after a successful login.
    protected async Task StartSignalRWithTokenAsync(string accessToken)
    {
        SignalRFixture.UseAccessToken(accessToken);
        await SignalRFixture.StartAsync();
        SignalRFixture.ClearReceivedMessages();
    }
}
