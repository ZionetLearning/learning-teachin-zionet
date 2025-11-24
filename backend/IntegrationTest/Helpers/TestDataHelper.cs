using IntegrationTests.Models;
using Manager.Models.Users;
using System.Security.Cryptography;

public static class TestDataHelper
{
    // Reserve a high range to avoid collisions with "real" data
    private const int MinTestId = 1_000_000;

    // Consistent password for all test users
    public const string DefaultTestPassword = "Test123!";
    
    // Test user default names
    public const string TestUserFirstName = "Test";
    public const string TestUserLastName = "User";

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

    public static CreateUserRequest CreateUser(string role = "student", string? email = null)
    {
        return new CreateUserRequest
        {
            UserId = Guid.NewGuid(),
            Email = email ?? $"user_{Guid.NewGuid():N}@test.com",
            FirstName = TestUserFirstName,
            LastName = TestUserLastName,
            Password = DefaultTestPassword,
            Role = role
        };
    }
    
    public static CreateUserRequest CreateUserWithFixedEmail(string? email = null)
    {
        return new CreateUserRequest
        {
            UserId = Guid.NewGuid(),
            Email = email ?? $"dup_{Guid.NewGuid()}@test.com",
            FirstName = TestUserFirstName,
            LastName = TestUserLastName,
            Password = DefaultTestPassword,
            Role = "student"
        };
    }
}
