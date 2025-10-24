using IntegrationTests.Constants;
using Manager.Models.Auth;
using Manager.Models.Users;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests.Fixtures;

public class HttpClientFixture : IAsyncLifetime
{
    private readonly ConcurrentDictionary<Role, (string Email, string Password, Guid UserId)> _globalUsers;
    private readonly ConcurrentDictionary<Role, string> _tokenCache = new();

    public HttpClient Client { get; }
    
    public const string GlobalTestUserPassword = "StrongPass!1";
    private const string TestUserFirstName = "Test-User-FirstName";
    private const string TestUserLastName = "Test-User-LastName";

    public HttpClientFixture()
    {
        var cfg = BuildConfig();

        var baseUrl = cfg.GetSection("TestSettings")["ApiBaseUrl"]
            ?? throw new InvalidOperationException(
                "TestSettings:ApiBaseUrl is missing. Add it to appsettings.json or appsettings.Local.json.");

        var handler = new HttpClientHandler();

        Client = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(40)
        };

        _globalUsers = new(new Dictionary<Role, (string Email, string Password, Guid UserId)>
        {
            { Role.Admin,   ("admin-test-user@testmail",   GlobalTestUserPassword, CreateDeterministicGuid("admin-test-user@sdkcrlscd")) },
            { Role.Teacher, ("teacher-test-user@testmail", GlobalTestUserPassword, CreateDeterministicGuid("teacher-test-user@sdkcrlscd")) },
            { Role.Student, ("student-test-user@testmail", GlobalTestUserPassword, CreateDeterministicGuid("student-test-user@sdkcrlscd")) },
        });
    }

    private static IConfigurationRoot BuildConfig() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

    private static Guid CreateDeterministicGuid(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));

        // Take the first 16 bytes (128 bits) of the 32-byte SHA256 hash
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);

        return new Guid(guidBytes);
    }

    public async Task InitializeAsync()
    {
        foreach (var (role, creds) in _globalUsers)
            await EnsureUserExistsAsync(creds.Email, creds.Password, creds.UserId, role);
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs in as a predefined test user of a given role and sets the default Authorization header.
    /// </summary>
    public async Task LoginAsync(Role role)
    {
        var token = await GetOrCreateTokenAsync(role);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Gets the user information for a predefined test user of a given role.
    /// Returns UserInfo record containing UserId, Email, and Role.
    /// </summary>
    public UserInfo GetUserInfo(Role role)
    {
        if (!_globalUsers.TryGetValue(role, out var userInfo))
            throw new InvalidOperationException($"User for role {role} has not been initialized.");

        return new UserInfo(userInfo.UserId, userInfo.Email, role);
    }

    /// <summary>
    /// Creates a temporary user with a random username and returns its username and token.
    /// Automatically logs in with this new user.
    /// </summary>
    public async Task<(string Username, string Token)> CreateEphemeralUserAsync(Role role)
    {
        var username = $"{role.ToString().ToLower()}-test-{Guid.NewGuid():N}";
        const string password = "TempPass!123";

        await RegisterUserAsync(username, password, role);
        var token = await LoginInternalAsync(username, password);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return (username, token);
    }

    /// <summary>
    /// Clears the Authorization header, effectively logging out for this HttpClient.
    /// </summary>
    public void ClearSession() =>
        Client.DefaultRequestHeaders.Authorization = null;

    // Internal helpers

    private async Task<string> GetOrCreateTokenAsync(Role role)
    {
        if (_tokenCache.TryGetValue(role, out var existing))
            return existing;

        if (!_globalUsers.TryGetValue(role, out var userInfo))
            throw new InvalidOperationException($"User for role {role} has not been initialized.");

        var token = await LoginInternalAsync(userInfo.Email, userInfo.Password);
        _tokenCache.TryAdd(role, token);

        return token;
    }

    private async Task EnsureUserExistsAsync(string email, string password, Guid userId, Role role)
    {
        var res = await Client.PostAsJsonAsync(UserRoutes.UserBase, new CreateUser
        {
            UserId = userId,
            Email = email,
            Password = password,
            FirstName = TestUserFirstName,
            LastName = TestUserLastName,
            Role = role.ToString()
        });

        if (res.StatusCode == HttpStatusCode.Conflict)
            return; // already exists — OK

        res.EnsureSuccessStatusCode();
    }

    private async Task RegisterUserAsync(string email, string password, Role role)
    {
        var res = await Client.PostAsJsonAsync(UserRoutes.UserBase, new CreateUser
        {
            UserId = Guid.NewGuid(),
            Email = email,
            Password = password,
            FirstName = TestUserFirstName,
            LastName = TestUserLastName,
            Role = role.ToString()
        });

        if (res.StatusCode == HttpStatusCode.Conflict)
            return; // already exists — OK

        res.EnsureSuccessStatusCode();
    }

    private async Task<string> LoginInternalAsync(string email, string password)
    {
        var res = await Client.PostAsJsonAsync(AuthRoutes.Login, new LoginRequest
        {
            Email = email,
            Password = password
        });
        res.EnsureSuccessStatusCode();

        var payload = await res.Content.ReadFromJsonAsync<LoginResponse>();
        return payload?.AccessToken ?? throw new InvalidOperationException("No access token returned from login.");
    }
}

public record UserInfo(Guid UserId, string Email, Role Role);

public record LoginResponse(string AccessToken, string RefreshToken);
