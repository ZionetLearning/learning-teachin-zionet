using IntegrationTests.Constants;
using IntegrationTests.Models.Auth;
using Manager.Constants;
using Manager.Models.Auth;
using Manager.Models.Users;
using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests.Fixtures;

public class TestUserFixture : IAsyncLifetime
{
    public CreateUser TestUser { get; private set; } = null!;
    private readonly HttpClient _client;

    public TestUserFixture(HttpTestFixture httpFixture)
    {
        _client = httpFixture.Client;
    }

    public async Task InitializeAsync()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var password = $"Test-{Guid.NewGuid():N}";

        // For now, the default role is Student
        Role defaultRole = Role.Admin;

        TestUser = new CreateUser
        {
            UserId = Guid.NewGuid(),
            Email = email,
            Password = password,
            FirstName = "Test-User-FirstName",
            LastName = "Test-User-LastName",
            Role = defaultRole.ToString()
        };

        // Create the test user in DB
        var response = await _client.PostAsJsonAsync(UserRoutes.UserBase, TestUser);
        response.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        try
        {
            // Log in to get access token
            var loginRequest = new LoginRequest
            {
                Email = TestUser.Email,
                Password = TestUser.Password
            };
            var loginResponse = await _client.PostAsJsonAsync(AuthRoutes.Login, loginRequest);
            loginResponse.EnsureSuccessStatusCode();

            var body = await loginResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AccessTokenResponse>(body)
                         ?? throw new InvalidOperationException("Invalid JSON response");

            var accessToken = result.AccessToken;
            var refreshToken = CookieHelper.ExtractCookieFromHeaders(loginResponse, AuthSettings.RefreshTokenCookieName);


            // Set the Authorization header on the shared client
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Clean up the test user from DB
            var deleteResponse = await _client.DeleteAsync(UserRoutes.UserById(TestUser.UserId));
            deleteResponse.EnsureSuccessStatusCode();

            // Clean up the refreshSession DB
            var logoutRequest = new HttpRequestMessage(HttpMethod.Post, AuthRoutes.Logout);
            logoutRequest.Headers.Add("Cookie", $"{AuthSettings.RefreshTokenCookieName}={refreshToken}");

            var logoutResponse = await _client.SendAsync(logoutRequest);
            logoutResponse.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Error during TestUserFixture cleanup: {ex.Message}");
        }

    }
}
