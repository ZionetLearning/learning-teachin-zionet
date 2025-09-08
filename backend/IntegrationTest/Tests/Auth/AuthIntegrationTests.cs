using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using Manager.Constants;
using Manager.Models.Auth;
using Manager.Models.Users;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
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

        var response = await LoginAsync(user.Email, user.Password);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accessToken = await ExtractAccessToken(response);
        accessToken.Should().NotBeNullOrWhiteSpace();

        var (refreshToken, csrfToken) = ExtractTokens(response);
        refreshToken.Should().NotBeNullOrWhiteSpace();
        csrfToken.Should().NotBeNullOrWhiteSpace();

        // Clear the refreshSessions
        var logoutResponse = await LogoutAsync(refreshToken);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    [Theory(DisplayName = "Login fails with 401 on invalid credentials")]
    [InlineData("invalid@example.com", "VALID")]       // Invalid email
    [InlineData("VALID", "wrongpass")]                 // Invalid password
    [InlineData("invalid@example.com", "wrongpass")]   // Both invalid
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsInvalid(string email, string password)
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var actualEmail = email == "VALID" ? user.Email : email;
        var actualPassword = password == "VALID" ? user.Password : password;

        var response = await LoginAsync(actualEmail, actualPassword);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact(DisplayName = "Logout clears refresh and csrf cookies")]
    public async Task Logout_ShouldClearCookies()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var loginResponse = await LoginAsync(user.Email, user.Password);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var (refreshToken, _) = ExtractTokens(loginResponse);

        // Clear the refreshSessions
        var logoutResponse = await LogoutAsync(refreshToken!);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cookies = logoutResponse.Headers.GetValues(TestConstants.SetCookie).ToList();
        cookies.Should().Contain(c => c.Contains($"{TestConstants.RefreshToken}=;"));
        cookies.Should().Contain(c => c.Contains($"{TestConstants.CsrfToken}=;"));
    }


    [Fact(DisplayName = "Refresh with invalid refresh token should return 401 Unauthorized")]
    public async Task Refresh_ShouldReturnUnauthorized()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var loginResponse = await LoginAsync(user.Email, user.Password);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var (refreshToken, csrfToken) = ExtractTokens(loginResponse);
        refreshToken.Should().NotBeNullOrWhiteSpace();
        csrfToken.Should().NotBeNullOrWhiteSpace();

        // Create a new HttpClient that does not send stored cookies (works in HTTPS)
        var handler = new HttpClientHandler
        {
            UseCookies = false
        };

        using var isolatedClient = new HttpClient(handler)
        {
            BaseAddress = Client.BaseAddress
        };

        var request = new HttpRequestMessage(HttpMethod.Post, AuthRoutes.Refresh)
        {
            Headers =
        {
            { "X-CSRF-Token", csrfToken! },
            { "Cookie", "refreshToken=InvalidToken" }
        }
        };

        var refreshResponse = await isolatedClient.SendAsync(request);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var logoutResponse = await LogoutAsync(refreshToken!);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }



    [Fact(DisplayName = "Access token allows access to protected endpoint")]
    public async Task AccessToken_ShouldAllowAccessToProtectedEndpoint()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var loginResponse = await LoginAsync(user.Email, user.Password);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await ExtractAccessToken(loginResponse);
        var (refreshToken, _) = ExtractTokens(loginResponse);

        var request = new HttpRequestMessage(HttpMethod.Get, AuthRoutes.Protected);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Clear the refreshSessions
        var logoutResponse = await LogoutAsync(refreshToken!);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
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
                Password = user.Password
            })
        };

        // Initial login with fingerprint A
        request.Headers.Add("x-fingerprint", "fingerprint-A");
        request.Headers.Add("User-Agent", "Test-UA");

        var loginResponse = await Client.SendAsync(request);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var (refreshToken, csrfToken) = ExtractTokens(loginResponse);

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

        // Clear the refreshSessions
        var logoutResponse = await LogoutAsync(refreshToken!);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    [Fact(DisplayName = "Refresh after logout should fail")]
    public async Task Refresh_AfterLogout_ShouldReturnUnauthorized()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var loginResponse = await LoginAsync(user.Email, user.Password);
        var (refreshToken, csrfToken) = ExtractTokens(loginResponse);

        // Logout
        var logoutResponse = await LogoutAsync(refreshToken!);
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

        var loginResponse = await LoginAsync(user.Email, user.Password);
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
        var logoutResponse = await LogoutAsync(newRefreshToken!);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logoutCookies = logoutResponse.Headers.GetValues($"{TestConstants.SetCookie}").ToList();
        logoutCookies.Should().Contain(c => c.Contains($"{TestConstants.RefreshToken}=;"));
        logoutCookies.Should().Contain(c => c.Contains($"{TestConstants.CsrfToken}=;"));

    }


    [Fact(DisplayName = "Access token should contain correct userid and role claims")]
    public async Task AccessToken_ShouldContainUserIdAndRoleClaims()
    {
        var user = _sharedFixture.UserFixture.TestUser;

        var response = await LoginAsync(user.Email, user.Password);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var (refreshToken, _) = ExtractTokens(response);

        var token = await ExtractAccessToken(response);
        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TestConstants.UserId)?.Value;
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TestConstants.Role)?.Value;

        userIdClaim.Should().NotBeNullOrWhiteSpace("userId should be present in the token");
        roleClaim.Should().NotBeNullOrWhiteSpace("Role should be present in the token");

        userIdClaim.Should().Be(user.UserId.ToString());
        roleClaim.Should().Be(user.Role.ToString());

        // Clear the refreshSessions
        var logoutResponse = await LogoutAsync(refreshToken!);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    [Theory(DisplayName = "JWT contains correct role for all defined roles")]
    [InlineData(Role.Admin)]
    [InlineData(Role.Teacher)]
    [InlineData(Role.Student)]
    public async Task Jwt_ShouldContainCorrectRole(Role role)
    {
        var newUser = new UserModel
        {
            UserId = Guid.NewGuid(),
            Email = $"role-test-{Guid.NewGuid():N}@example.com",
            Password = "Test123!",
            FirstName = "Theory",
            LastName = "User",
            Role = role
        };

        var registerResponse = await Client.PostAsJsonAsync(UserRoutes.UserBase, newUser);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await LoginAsync(newUser.Email, newUser.Password);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var (refreshToken, _) = ExtractTokens(loginResponse);

        var accessToken = await ExtractAccessToken(loginResponse);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == AuthSettings.RoleClaimType)?.Value;

        roleClaim.Should().Be(role.ToString());

        // Set the Authorization header on the shared client
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        // Clean up the test user from DB
        var deleteResponse = await Client.DeleteAsync(UserRoutes.UserById(newUser.UserId));
        deleteResponse.EnsureSuccessStatusCode();

        // Clear the refreshSessions
        var logoutResponse = await LogoutAsync(refreshToken!);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

}