using IntegrationTests.Models;

namespace IntegrationTests.Helpers;

public static class TestDataHelper
{
    public static TaskModel CreateValidTask(int id = 1, string name = "Test Task", string payload = "Sample payload data")
    {
        return new TaskModel
        {
            Id = id,
            Name = name,
            Payload = payload
        };
    }

    public static TaskModel CreateInvalidTask()
    {
        return new TaskModel
        {
            Id = -1,
            Name = string.Empty,
            Payload = string.Empty
        };
    }
}
