//using Microsoft.EntityFrameworkCore;
using IntegrationTests.Models;
//using IntegrationTests.DB;
using System.Security.Cryptography;

public static class TestDataHelper
{
    // Reserve a high range to avoid collisions with “real” data
    private const int MinTestId = 1_000_000;

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

    public static TaskModel CreateFixedIdTask() => new()
    {
        Id = 888,
        Name = $"Task-888",
        Payload = $"Payload-{888}"
    };
}
