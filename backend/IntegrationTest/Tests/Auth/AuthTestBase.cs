using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Models.Auth;
using IntegrationTests.Models.Auth;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;
using IntegrationTests.Constants;

namespace IntegrationTests.Tests.Auth;

public abstract class AuthTestBase : IntegrationTestBase
{
    protected AuthTestBase(HttpTestFixture fixture, ITestOutputHelper outputHelper, SignalRTestFixture signalRFixture)
        : base(fixture, outputHelper, signalRFixture)
    {
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


}
