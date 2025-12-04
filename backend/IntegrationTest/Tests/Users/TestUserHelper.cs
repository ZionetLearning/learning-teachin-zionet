using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Models.Auth;
using Manager.Models.Auth;
using Manager.Models.Users;

namespace IntegrationTests.Tests.Users;

public class TestUserHelper
{
    private readonly HttpClient _client;

    public TestUserHelper(HttpClient client)
    {
        _client = client;
    }

    // exactly the same shape you had in the test
    public static CreateUserRequest NewUser(Role role) => new()
    {
        UserId = Guid.NewGuid(),
        Email = $"{role.ToString().ToLower()}-{Guid.NewGuid():N}@example.com",
        Password = "Test123!",
        FirstName = role.ToString(),
        LastName = "Auto",
        Role = role.ToString()
    };

    public async Task<Guid> CreateUserAsync(Role role)
    {
        var u = NewUser(role);
        var res = await _client.PostAsJsonAsync(UserRoutes.UserBase, u);
        res.EnsureSuccessStatusCode();
        return u.UserId;
    }

    public async Task<string> LoginAndGetTokenAsync(string email, string password)
    {
        var res = await _client.PostAsJsonAsync(AuthRoutes.Login, new LoginRequest { Email = email, Password = password });
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<AccessTokenResponse>(body) ?? throw new InvalidOperationException("Invalid JSON");
        return dto.AccessToken;
    }

    public async Task<(Guid id, string email, string password)> CreateAndLogin(Role role)
    {
        var u = NewUser(role);
        var res = await _client.PostAsJsonAsync(UserRoutes.UserBase, u);
        res.EnsureSuccessStatusCode();

        // perform login to validate credentials flow (token not returned here to keep signature identical)
        _ = await LoginAndGetTokenAsync(u.Email, u.Password);

        return (u.UserId, u.Email, u.Password);
    }

    public void UseBearer(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task CleanupUser(Guid id)
    {
        var del = await _client.DeleteAsync(UserRoutes.UserById(id));
        if (del.StatusCode != HttpStatusCode.OK && del.StatusCode != HttpStatusCode.NotFound)
            del.EnsureSuccessStatusCode();
    }
}
