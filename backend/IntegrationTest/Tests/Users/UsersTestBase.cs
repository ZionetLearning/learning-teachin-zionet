using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Constants;
using Manager.Models.Users;
using Xunit.Abstractions;
using System.Net.Http.Json;

namespace IntegrationTests.Tests.Users;

/// <summary>
/// Base for Users integration tests.
/// Relies on PerTestUserFixture so each test has isolated users.
/// </summary>
[Collection("Per-test user collection")]
public abstract class UsersTestBase(
    PerTestUserFixture perUserFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(perUserFixture.HttpFixture, outputHelper, signalRFixture)
{
    protected PerTestUserFixture PerUserFixture { get; } = perUserFixture;

    /// <summary>
    /// Creates a user (default role: student) and logs them in.
    /// </summary>
    protected Task<UserData> CreateUserAsync(
        string role = "student",
        string? email = null,
        string? acceptLanguage = "en-US")
    {
        var parsedRole = Enum.TryParse<Role>(role, true, out var r) ? r : Role.Student;
        return PerUserFixture.CreateAndLoginAsync(parsedRole, email);
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