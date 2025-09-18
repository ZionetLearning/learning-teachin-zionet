using IntegrationTests.Constants;
using IntegrationTests.Models.Auth;
using Manager.Models.Auth;
using Manager.Models.Users;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests.Fixtures;

public class PerTestUserFixture : IAsyncLifetime
{
    private readonly HttpTestFixture _httpFixture;

    public HttpTestFixture HttpFixture => _httpFixture;
    public HttpClient Client => _httpFixture.Client;

    private readonly List<Guid> _createdUserIds = new();

    public PerTestUserFixture()
    {
        _httpFixture = new HttpTestFixture();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var id in _createdUserIds)
        {
            try { await Client.DeleteAsync(UserRoutes.UserById(id)); } catch { }
        }

        _httpFixture.Dispose();
    }

    /// <summary>
    /// Creates a new user with the given role, logs in, sets the bearer token,
    /// and returns the created user as UserData.
    /// </summary>
    public async Task<UserData> CreateAndLoginAsync(Role role, string? email = null)
    {
        var newUser = new UserModel
        {
            UserId = Guid.NewGuid(),
            Email = email ?? $"test-{Guid.NewGuid():N}@example.com",
            Password = "Test123!",
            FirstName = "PerTest",
            LastName = "User",
            Role = role
        };

        var createRes = await Client.PostAsJsonAsync(UserRoutes.UserBase, newUser);
        createRes.EnsureSuccessStatusCode();

        _createdUserIds.Add(newUser.UserId);

        // login
        var loginReq = new LoginRequest { Email = newUser.Email, Password = newUser.Password };
        var loginRes = await Client.PostAsJsonAsync(AuthRoutes.Login, loginReq);
        loginRes.EnsureSuccessStatusCode();

        var body = await loginRes.Content.ReadAsStringAsync();
        var tokenRes = JsonSerializer.Deserialize<AccessTokenResponse>(body)
                       ?? throw new InvalidOperationException("Invalid login response");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenRes.AccessToken);

        return new UserData
        {
            UserId = newUser.UserId,
            Email = newUser.Email,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Role = newUser.Role,
            PreferredLanguageCode = SupportedLanguage.en,
            HebrewLevelValue = HebrewLevel.beginner
        };
    }
}