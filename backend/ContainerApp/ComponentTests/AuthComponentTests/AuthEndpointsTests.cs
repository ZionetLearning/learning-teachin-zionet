using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Manager.Models.Auth;


public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });
    }

    [Fact(DisplayName = "LoginAsync returns access token and sets refresh and csrf cookies")]
    public async Task Login_SetsRefreshAndCsrfCookies_WhenValid()
    {
        var loginRequest = new LoginRequest
        {
            Email = "test@email.com",
            Password = "pass"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert: Body contains access token
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.True(json.TryGetProperty("accessToken", out var accessToken));
        Assert.False(string.IsNullOrWhiteSpace(accessToken.GetString()));

        // Assert: Cookies set
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        var cookieList = cookies.ToList();

        Assert.Contains(cookieList, c => c.Contains("refresh-token=") && !c.Contains("Max-Age=0"));
        Assert.Contains(cookieList, c => c.Contains("csrf-token=") && !c.Contains("Max-Age=0"));
    }


    [Fact(DisplayName = "LogoutAsync clears refresh and CSRF cookies and returns 200")]
    public async Task LogoutAsync_ShouldClearCookiesAndReturnOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");

        // Simulate valid refresh + csrf cookies in the request
        request.Headers.Add("Cookie",
            $"{Manager.Constants.AuthSettings.RefreshTokenCookieName}=test-refresh-token; " +
            $"{Manager.Constants.AuthSettings.CsrfTokenCookieName}=test-csrf-token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders));

        // There should be cookies being cleared
        var cookies = setCookieHeaders.ToList();
        Assert.Contains(cookies, c => c.Contains("refresh-token=;") && c.Contains("Max-Age=0"));
        Assert.Contains(cookies, c => c.Contains("csrf-token=;") && c.Contains("Max-Age=0"));
    }
}
