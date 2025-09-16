namespace IntegrationTests.Constants;

public static class ApiRoutes
{
    // -------- Tasks --------

    public const string Task = "tasks-manager/task";
    public static string TaskById(int id) => $"tasks-manager/task/{id}";
    public static string UpdateTaskName(int id, string name) => $"tasks-manager/task/{id}/{name}";

    // -------- Users --------
    public const string User = "users-manager/user";
    public static string UserById(Guid userId) => $"users-manager/user/{userId}";
    public const string UserList = "users-manager/user-list";
    public const string TasksList = "tasks-manager/tasks";
}
