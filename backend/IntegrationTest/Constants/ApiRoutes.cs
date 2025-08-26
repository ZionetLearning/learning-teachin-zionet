namespace IntegrationTests.Constants;

public static class ApiRoutes
{
    public const string Task = "tasks-manager/task";
    public static string TaskById(int id) => $"tasks-manager/task/{id}";
    public static string UpdateTaskName(int id, string name) => $"tasks-manager/task/{id}/{name}";
}
