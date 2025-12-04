using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Models.Users;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Users;

/// <summary>
/// Base for Users integration tests.
/// Uses HttpClientFixture for consistent authentication pattern.
/// </summary>
[Collection("IntegrationTests")]
public abstract class UsersTestBase(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(httpClientFixture, outputHelper, signalRFixture)
{
    public override async Task InitializeAsync()
    {
        // Don't login by default - let tests choose which role to use
        SignalRFixture.ClearReceivedMessages();
    }

    /// <summary>
    /// Creates a user (default role: student) and logs them in.
    /// Returns UserData for the created user.
    /// </summary>
    protected async Task<GetUserResponse> CreateUserAsync(
        string role = "student",
        string? email = null,
        string? acceptLanguage = "en-US")
    {
        var parsedRole = Enum.TryParse<Role>(role, true, out var r) ? r : Role.Student;
        email ??= $"{role}-{Guid.NewGuid():N}@example.com";

        var user = new UserModel
        {
            UserId = Guid.NewGuid(),
            Email = email,
            Password = TestDataHelper.DefaultTestPassword,
            FirstName = TestDataHelper.TestUserFirstName,
            LastName = TestDataHelper.TestUserLastName,
            Role = parsedRole
        };

        var createRes = await Client.PostAsJsonAsync(Constants.UserRoutes.UserBase, user);
        createRes.EnsureSuccessStatusCode();

        // Login
        var loginReq = new Manager.Models.Auth.LoginRequest { Email = user.Email, Password = TestDataHelper.DefaultTestPassword };
        var loginRes = await Client.PostAsJsonAsync(Constants.AuthRoutes.Login, loginReq);
        loginRes.EnsureSuccessStatusCode();

        var body = await loginRes.Content.ReadAsStringAsync();
        var tokenRes = System.Text.Json.JsonSerializer.Deserialize<Models.Auth.AccessTokenResponse>(body)
                       ?? throw new InvalidOperationException("Invalid login response");

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenRes.AccessToken);

        return new GetUserResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = parsedRole,
            PreferredLanguageCode = SupportedLanguage.en,
            HebrewLevelValue = HebrewLevel.beginner
        };
    }

    /// <summary>
    /// Creates a user with the specified role and logs them in.
    /// Clears any existing authorization before login.
    /// Returns UserData for the created user.
    /// </summary>
    protected async Task<GetUserResponse> CreateAndLoginAsync(Role role, string? email = null)
    {
        email ??= $"{role.ToString().ToLower()}-{Guid.NewGuid():N}@example.com";

        var user = new UserModel
        {
            UserId = Guid.NewGuid(),
            Email = email,
            Password = TestDataHelper.DefaultTestPassword,
            FirstName = TestDataHelper.TestUserFirstName,
            LastName = TestDataHelper.TestUserLastName,
            Role = role
        };

        var createRes = await Client.PostAsJsonAsync(Constants.UserRoutes.UserBase, user);
        createRes.EnsureSuccessStatusCode();

        // Clear previous auth before logging in
        Client.DefaultRequestHeaders.Authorization = null;

        // Login
        var loginReq = new Manager.Models.Auth.LoginRequest { Email = user.Email, Password = TestDataHelper.DefaultTestPassword };
        var loginRes = await Client.PostAsJsonAsync(Constants.AuthRoutes.Login, loginReq);
        loginRes.EnsureSuccessStatusCode();

        var body = await loginRes.Content.ReadAsStringAsync();
        var tokenRes = System.Text.Json.JsonSerializer.Deserialize<Models.Auth.AccessTokenResponse>(body)
                       ?? throw new InvalidOperationException("Invalid login response");

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenRes.AccessToken);

        return new GetUserResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role,
            PreferredLanguageCode = SupportedLanguage.en,
            HebrewLevelValue = HebrewLevel.beginner
        };
    }

    /// <summary>
    /// Builds a request with an optional Accept-Language header.
    /// </summary>
    protected HttpRequestMessage BuildRequest(string url, object body, string? acceptLanguage = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };

        if (!string.IsNullOrWhiteSpace(acceptLanguage))
            request.Headers.Add("Accept-Language", acceptLanguage);

        return request;
    }
}