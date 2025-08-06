using IntegrationTests.Models;

namespace IntegrationTests.Helpers;

public static class TaskTestData
{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public static IEnumerable<object[]> InvalidTasks =>
        new List<object[]>
        {
            new object[] { null },
            new object[] { new TaskModel { Id = -1, Name = "", Payload = "" } },
            new object[] { new TaskModel { Id = 0, Name = null!, Payload = "Test" } }
        };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
}
