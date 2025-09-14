using IntegrationTests.Models;
using Manager.Models.Users;
using System.Security.Cryptography;

public static class TestDataHelper
{
    // Reserve a high range to avoid collisions with “real” data
    private const int MinTestId = 1_000_000;
    // Shared password for all test users
    public const string DefaultPassword = "Passw0rd!";

    public static TaskModel CreateRandomTask()
    {
        var id = MinTestId + RandomNumberGenerator.GetInt32(1, int.MaxValue - MinTestId);

        return new TaskModel
        {
            Id = id,
            Name = $"Task-{id}",
            Payload = $"Payload-{Guid.NewGuid():N}"
        };
    }

    public static TaskModel CreateFixedIdTask(int id = 888) => new()
    {
        Id = id,
        Name = $"Task-{id}",
        Payload = $"Payload-{id}"
    };

    public static CreateUser CreateUser(string role = "student", string? email = null)
    {
        return new CreateUser
        {
            UserId = Guid.NewGuid(),
            Email = email ?? $"user_{Guid.NewGuid():N}@test.com",
            FirstName = "Test",
            LastName = "User",
            Password = "Passw0rd!",
            Role = role
        };
    }
    public static CreateUser CreateUserWithFixedEmail(string? email = null)
    {
        return new CreateUser
        {
            UserId = Guid.NewGuid(),
            Email = email ?? $"dup_{Guid.NewGuid()}@test.com",
            FirstName = "Test",
            LastName = "User",
            Password = DefaultPassword,
            Role = "student"
        };
    }
}
