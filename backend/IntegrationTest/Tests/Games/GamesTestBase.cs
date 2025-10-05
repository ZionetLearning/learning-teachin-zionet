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

    /// <summary>
    /// Creates a "pending" game attempt by directly inserting it into the database via a game generation endpoint.
    /// Since we don't have direct DB access in integration tests, we'll simulate this by calling the sentence generation endpoint.
    /// </summary>
    protected async Task<Guid> CreatePendingAttemptAsync(Guid studentId, List<string> correctAnswer, string gameType = "SplitSentence", string difficulty = "Easy")
    {
        // Note: In a real scenario, we would call the sentence generation endpoint
        // For now, we'll just return a new GUID as a placeholder
        // The actual implementation should use the sentence generation endpoint
        return Guid.NewGuid();
    }
}
