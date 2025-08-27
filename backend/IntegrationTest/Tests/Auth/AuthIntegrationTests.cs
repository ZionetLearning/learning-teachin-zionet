using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using Manager.Models.Auth;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Auth;


[Collection("Shared test collection")]
public class AuthIntegrationTests : AuthTestBase
{
    private readonly SharedTestFixture _sharedFixture;

    public AuthIntegrationTests(
        SharedTestFixture sharedFixture,
        ITestOutputHelper outputHelper
    ) : base(sharedFixture.HttpFixture, outputHelper, new SignalRTestFixture())
    {
        _sharedFixture = sharedFixture;
    }

    [Fact(DisplayName = "Login returns access token and sets cookies")]
    public async Task Login_ShouldReturnAccessTokenAndSetCookies()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var response = await LoginAsync(user.Email, user.PasswordHash);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accessToken = await ExtractAccessToken(response);
        accessToken.Should().NotBeNullOrWhiteSpace();

        var (refreshToken, csrfToken) = ExtractTokens(response);
        refreshToken.Should().NotBeNullOrWhiteSpace();
        csrfToken.Should().NotBeNullOrWhiteSpace();
    }


    [Theory(DisplayName = "Login fails with 401 on invalid credentials")]
    [InlineData("invalid@example.com", "VALID")]       // Invalid email
    [InlineData("VALID", "wrongpass")]                 // Invalid password
    [InlineData("invalid@example.com", "wrongpass")]   // Both invalid
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsInvalid(string email, string password)
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var actualEmail = email == "VALID" ? user.Email : email;
        var actualPassword = password == "VALID" ? user.PasswordHash : password;

        var response = await LoginAsync(actualEmail, actualPassword);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact(DisplayName = "Logout clears refresh and csrf cookies")]
    public async Task Logout_ShouldClearCookies()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var loginResponse = await LoginAsync(user.Email, user.PasswordHash);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logoutResponse = await Client.PostAsync(AuthRoutes.Logout, null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cookies = logoutResponse.Headers.GetValues(TestConstants.SetCookie).ToList();
        cookies.Should().Contain(c => c.Contains($"{TestConstants.RefreshToken}=;"));
        cookies.Should().Contain(c => c.Contains($"{TestConstants.CsrfToken}=;"));
    }


    [Fact(DisplayName = "Refresh with invalid refresh token should fail")]
    public async Task Refresh_ShouldReturnUnauthorized()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var loginResponse = await LoginAsync(user.Email, user.PasswordHash);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var (_, csrfToken) = ExtractTokens(loginResponse);
        csrfToken.Should().NotBeNullOrWhiteSpace();

        var request = new HttpRequestMessage(HttpMethod.Post, AuthRoutes.Refresh)
        {
            Headers =
            {
                { "X-CSRF-Token", csrfToken! },
                { "Cookie", $"refreshToken=InvalidToken" }
            }
        };
        var refreshResponse = await Client.SendAsync(request);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    

    [Fact(DisplayName = "Access token allows access to protected endpoint")]
    public async Task AccessToken_ShouldAllowAccessToProtectedEndpoint()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var loginResponse = await LoginAsync(user.Email, user.PasswordHash);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await ExtractAccessToken(loginResponse);

        var request = new HttpRequestMessage(HttpMethod.Get, AuthRoutes.Protected);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    [Fact(DisplayName = "Refresh fails with mismatched fingerprint")]
    public async Task Refresh_ShouldFail_WhenFingerprintMismatch()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var request = new HttpRequestMessage(HttpMethod.Post, AuthRoutes.Login)
        {
            Content = JsonContent.Create(new LoginRequest
            {
                Email = user.Email,
                Password = user.PasswordHash
            })
        };

        // Initial login with fingerprint A
        request.Headers.Add("x-fingerprint", "fingerprint-A");
        request.Headers.Add("User-Agent", "Test-UA");

        var loginResponse = await Client.SendAsync(request);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var (_, csrfToken) = ExtractTokens(loginResponse);

        // Now send refresh request with DIFFERENT fingerprint
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, AuthRoutes.Refresh);
        refreshRequest.Headers.Add("x-fingerprint", "fingerprint-B");
        refreshRequest.Headers.Add("X-CSRF-Token", csrfToken!);
        refreshRequest.Headers.Add("Cookie", string.Join("; ", loginResponse.Headers
            .GetValues(TestConstants.SetCookie)
            .Where(c => c.StartsWith(TestConstants.RefreshToken) || c.StartsWith(TestConstants.CsrfToken))
            .Select(c => c.Split(';')[0]))); // Simulate sending both cookies

        var refreshResponse = await Client.SendAsync(refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact(DisplayName = "Refresh after logout should fail")]
    public async Task Refresh_AfterLogout_ShouldReturnUnauthorized()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var loginResponse = await LoginAsync(user.Email, user.PasswordHash);
        var (refreshToken, csrfToken) = ExtractTokens(loginResponse);

        // Logout
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, AuthRoutes.Logout);
        logoutRequest.Headers.Add("Cookie", $"{TestConstants.RefreshToken}={refreshToken}");
        var logoutResponse = await Client.SendAsync(logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Try to refresh after logout
        var refreshRequest = CreateRefreshRequest(refreshToken!, csrfToken!);

        var refreshResponse = await Client.SendAsync(refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact(DisplayName = "Successful full auth flow (register, login, access protected, refresh, logout)")]
    public async Task SuccessfulAuthFlow_ShouldSetCookiesAndAccessProtectedRoute()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var loginResponse = await LoginAsync(user.Email, user.PasswordHash);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var accessToken = await ExtractAccessToken(loginResponse);
        var (refreshToken, csrfToken) = ExtractTokens(loginResponse);

        // Access protected
        var protectedRequest = new HttpRequestMessage(HttpMethod.Get, AuthRoutes.Protected);
        protectedRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var protectedResponse = await Client.SendAsync(protectedRequest);
        protectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);


        var refreshRequest = CreateRefreshRequest(refreshToken!, csrfToken!);

        var refreshResponse = await Client.SendAsync(refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var newAccessToken = await ExtractAccessToken(refreshResponse);
        newAccessToken.Should().NotBeNullOrWhiteSpace();

        var newRefreshToken = CookieHelper.ExtractCookieFromHeaders(refreshResponse, TestConstants.RefreshToken);
        newRefreshToken.Should().NotBeNullOrWhiteSpace();

        // Logout
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, AuthRoutes.Logout);
        logoutRequest.Headers.Add("Cookie", $"{TestConstants.RefreshToken}={newRefreshToken}");

        var logoutResponse = await Client.SendAsync(logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logoutCookies = logoutResponse.Headers.GetValues($"{TestConstants.SetCookie}").ToList();
        logoutCookies.Should().Contain(c => c.Contains($"{TestConstants.RefreshToken}=;"));
        logoutCookies.Should().Contain(c => c.Contains($"{TestConstants.CsrfToken}=;"));

    }

}