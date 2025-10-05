using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Constants;
using Manager.Models.Users;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Games;

/// <summary>
/// Base for Games integration tests.
/// Relies on PerTestUserFixture so each test has isolated users.
/// </summary>
[Collection("Per-test user collection")]
public abstract class GamesTestBase(
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
        string? email = null)
    {
        var parsedRole = Enum.TryParse<Role>(role, true, out var r) ? r : Role.Student;
        return PerUserFixture.CreateAndLoginAsync(parsedRole, email);
    }
}
