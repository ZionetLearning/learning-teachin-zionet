using System.Net.Http.Headers;
using System.Net.Http.Json;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models.Auth;
using Manager.Models.Auth;
using Manager.Models.Users;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Users;

public abstract class UsersTestBase(
    MinimalSharedTestFixture minimalFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(minimalFixture.HttpFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    protected MinimalSharedTestFixture Shared { get; } = minimalFixture;
    private readonly List<Guid> _createdUserIds = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => CleanupAsync();

    private async Task CleanupAsync()
    {
        foreach (var id in _createdUserIds)
        {
            await Client.DeleteAsync(ApiRoutes.UserById(id));
        }
        _createdUserIds.Clear();
    }

    /// <summary>
    /// Creates a user via POST and returns the created user data.
    /// </summary>
    protected async Task<UserData> CreateUserAsync(
        string role = "student",
        string? email = null,
        string? acceptLanguage = "en-US")
    {
        var user = TestDataHelper.CreateUser(role: role, email: email);
        var request = BuildRequest(ApiRoutes.User, user, acceptLanguage);

        var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var created = await ReadAsJsonAsync<UserData>(response);
        _createdUserIds.Add(created!.UserId);
        return created!;
    }

    /// <summary>
    /// Creates and logs in a user, returning both the user and its token.
    /// </summary>
    protected async Task<(UserData user, string token)> CreateAndLoginUserAsync(
        string role = "student",
        string? email = null)
    {
        var user = await CreateUserAsync(role, email);

        var loginRequest = new LoginRequest
        {
            Email = user.Email,
            Password = TestDataHelper.DefaultPassword // make sure DefaultPassword = "Passw0rd!"
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.Login, loginRequest);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await ReadAsJsonAsync<AccessTokenResponse>(response);
        var token = tokenResponse!.AccessToken;

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return (user, token);
    }

    /// <summary>
    /// Builds a POST request with optional Accept-Language header.
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