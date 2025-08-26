using Manager.Models.Users;
using System.Net.Http.Json;

namespace IntegrationTests.Fixtures;

public class TestUserFixture : IAsyncLifetime
{
    public UserModel TestUser { get; private set; } = null!;
    private readonly HttpClient _client;

    public TestUserFixture(HttpTestFixture httpFixture)
    {
        _client = httpFixture.Client;
    }

    public async Task InitializeAsync()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var password = $"Test-{Guid.NewGuid():N}";

        TestUser = new UserModel
        {
            UserId = Guid.NewGuid(),
            Email = email,
            PasswordHash = password,
        };

        // Create the test user in DB
        var response = await _client.PostAsJsonAsync("/users-manager/user", TestUser);
        response.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        // Clean up the test user from DB
        var response = await _client.DeleteAsync($"/users-manager/user/{TestUser.UserId}");
        response.EnsureSuccessStatusCode();
    }
}
