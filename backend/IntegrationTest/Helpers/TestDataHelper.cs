using IntegrationTests.Models;
using System.Threading;

namespace IntegrationTests.Helpers;

public static class TestDataHelper
{
    private static int _nextId = 1;

    public static TaskModel CreateRandomTask()
    {
        var id = Interlocked.Increment(ref _nextId);
        return new TaskModel
        {
            Id = id,
            Name = $"Task-{Guid.NewGuid().ToString()[..6]}",
            Payload = $"Payload-{Guid.NewGuid()}"
        };
    }
}
