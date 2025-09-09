using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Models.Users;
using Xunit.Abstractions;
using System.Net.Http.Json;

namespace IntegrationTests.Tests.Users;

public abstract class UsersTestBase(
    SharedTestFixture sharedFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(sharedFixture.HttpFixture, outputHelper, signalRFixture)
{
    protected SharedTestFixture Shared { get; } = sharedFixture;
    public override Task InitializeAsync() => SuiteInit.EnsureAsync(Shared, SignalRFixture, OutputHelper);

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
        return created!;
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