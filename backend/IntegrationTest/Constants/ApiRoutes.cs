namespace IntegrationTests.Constants;

public static class ApiRoutes
{
    public const string Task = "task";
    public static string TaskById(int id) => $"task/{id}";
    public static string UpdateTaskName(int id, string name) => $"task/{id}/{name}";
}
