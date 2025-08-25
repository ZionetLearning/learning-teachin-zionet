using FluentAssertions;
using IntegrationTests.Infrastructure;
using Manager.Models.Auth;
using Manager.Models.Users;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;
namespace IntegrationTests.Tests.Auth;
using IntegrationTests.Constants;

public class AuthIntegrationTests(
    HttpTestFixture fixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : AuthTestBase(fixture, outputHelper, signalRFixture)
{
    
    [Fact(DisplayName = "Login returns access token and sets cookies")]
    public async Task Login_ShouldReturnAccessTokenAndSetCookies()
    {
        // Register user
        var user = await RegisterTestUserAsync();

        var loginRequest = new LoginRequest
        {
            Email = user.Email,
            Password = user.PasswordHash
        };

        var response = await Client.PostAsJsonAsync("/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.True(json.TryGetProperty("accessToken", out var accessToken));
        Assert.False(string.IsNullOrWhiteSpace(accessToken.GetString()));

        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        var cookieList = cookies.ToList();
        Assert.Contains(cookieList, c => c.Contains("refreshToken="));
        Assert.Contains(cookieList, c => c.Contains("csrfToken="));
        await DeleteTestUserAsync(user.UserId);
    }


    [Fact(DisplayName = "Login fails with 401 on invalid credentials")]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsInvalid()
    {
        var user = await RegisterTestUserAsync();

        var loginRequest = new LoginRequest
        {
            Email = "wrong@email.com",
            Password = user.PasswordHash
        };

        var response = await Client.PostAsJsonAsync("/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await DeleteTestUserAsync(user.UserId);
    }


    [Fact(DisplayName = "Logout clears refresh and csrf cookies")]
    public async Task Logout_ShouldClearCookies()
    {
        var user = await RegisterTestUserAsync();

        // Simulate login to get cookies
        var loginRequest = new LoginRequest 
        { 
            Email = user.Email, 
            Password = user.PasswordHash
        };
        // Call login
        var loginResponse = await Client.PostAsJsonAsync("/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Call logout
        var logoutResponse = await Client.PostAsync("/auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        Assert.True(logoutResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
        var cookieList = cookies.ToList();
        Assert.Contains(cookieList, c => c.Contains("refreshToken=;"));
        Assert.Contains(cookieList, c => c.Contains("csrfToken=;"));
        await DeleteTestUserAsync(user.UserId);
    }


    [Fact(DisplayName = "Refresh with invalid refresh token should fail")]
    public async Task Refresh_ShouldReturnUnauthorized()
    {
        // Register a user inside the DB
        var user = await RegisterTestUserAsync();

        // Login
        var loginRequest = new LoginRequest
        {
            Email = user.Email,
            Password = user.PasswordHash
        };

        var loginResponse = await Client.PostAsJsonAsync("/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Extract csrfToken from Set-Cookie
        var csrfToken = CookieHelper.ExtractCookieFromHeaders(loginResponse, "csrfToken");
        csrfToken.Should().NotBeNullOrWhiteSpace();

        // Send wrong refresh request
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh-tokens");
        refreshRequest.Headers.Add("X-CSRF-Token", csrfToken);
        refreshRequest.Headers.Add("Cookie", $"refreshToken=UnauthorizedToken");

        var refreshResponse = await Client.SendAsync(refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await DeleteTestUserAsync(user.UserId);
    }
    

    [Fact(DisplayName = "Refresh without CSRF token should fail")]
    public async Task Refresh_WithoutCsrf_ShouldReturnUnauthorized()
    {
        var user = await RegisterTestUserAsync();

        // Login
        var loginRequest = new LoginRequest
        {
            Email = user.Email,
            Password = user.PasswordHash
        };
        var loginResponse = await Client.PostAsJsonAsync("/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Attempt refresh with no CSRF header
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh-tokens");
        var refreshResponse = await Client.SendAsync(refreshRequest);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        await DeleteTestUserAsync(user.UserId);
    }


    [Fact(DisplayName = "Access token allows access to protected endpoint")]
    public async Task AccessToken_ShouldAllowAccessToProtectedEndpoint()
    {
        var user = await RegisterTestUserAsync();

        // Login 
        var loginRequest = new LoginRequest
        {
            Email = user.Email,
            Password = user.PasswordHash
        };

        var loginResponse = await Client.PostAsJsonAsync("/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await loginResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.TryGetProperty("accessToken", out var accessToken).Should().BeTrue();

        // Get the access token
        var token = accessToken.GetString();
        token.Should().NotBeNullOrWhiteSpace();

        // Use the token to access a protected endpoint
        var request = new HttpRequestMessage(HttpMethod.Get, "auth/protected");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var protectedResponse = await Client.SendAsync(request);

        protectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        await DeleteTestUserAsync(user.UserId);
    }


    [Fact(DisplayName = "Successful full auth flow (register, login, access protected, refresh, logout)")]
    public async Task SuccessfulAuthFlow_ShouldSetCookiesAndAccessProtectedRoute()
    {
        // Register user
        var user = await RegisterTestUserAsync();

        // Login
        var loginRequest = new LoginRequest
        {
            Email = user.Email,
            Password = user.PasswordHash
        };

        var loginResponse = await Client.PostAsJsonAsync("/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Extract access token
        var json = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var accessToken = json.RootElement.GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrWhiteSpace();

        // Exctract Refresh and CSRF cookie
        var refreshToken = CookieHelper.ExtractCookieFromHeaders(loginResponse, "refreshToken");
        refreshToken.Should().NotBeNullOrWhiteSpace();
        var csrfToken = CookieHelper.ExtractCookieFromHeaders(loginResponse, "csrfToken");
        csrfToken.Should().NotBeNullOrWhiteSpace();


        // Use access token to hit protected endpoint
        var protectedRequest = new HttpRequestMessage(HttpMethod.Get, "/auth/protected");
        protectedRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var protectedResponse = await Client.SendAsync(protectedRequest);
        protectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Refresh tokens using valid refresh cookie and CSRF header
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh-tokens");
        refreshRequest.Headers.Add("X-CSRF-Token", csrfToken);
        refreshRequest.Headers.Add("Cookie", $"refreshToken={refreshToken}; csrfToken={csrfToken}");

        var refreshResponse = await Client.SendAsync(refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Check the access token and refresh cookie are updated
        var refreshBody = JsonDocument.Parse(await refreshResponse.Content.ReadAsStringAsync());
        var newAccessToken = refreshBody.RootElement.GetProperty("accessToken").GetString();
        newAccessToken.Should().NotBeNullOrWhiteSpace();
        refreshToken = CookieHelper.ExtractCookieFromHeaders(refreshResponse, "refreshToken");
        refreshToken.Should().NotBeNullOrWhiteSpace();

        // Logout
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        logoutRequest.Headers.Add("Cookie", $"refreshToken={refreshToken}");
        var logoutResponse = await Client.SendAsync(logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logoutSetCookies = logoutResponse.Headers.GetValues("Set-Cookie").ToList();
        logoutSetCookies.Should().Contain(c => c.Contains("refreshToken=;"));
        logoutSetCookies.Should().Contain(c => c.Contains("csrfToken=;"));

        await DeleteTestUserAsync(user.UserId);
    }

  
    private async Task<UserModel> RegisterTestUserAsync()
    {
        var user = new UserModel
        {
            UserId = Guid.NewGuid(),
            Email = TestConstants._testUserEmail,
            PasswordHash = TestConstants._testUserPassword,
        };

        var response = await Client.PostAsJsonAsync("/user", user);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return user;
    }


    private async Task DeleteTestUserAsync(Guid userId)
    {
        var response = await Client.DeleteAsync($"/user/{userId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

}
