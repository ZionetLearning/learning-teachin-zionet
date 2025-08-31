namespace IntegrationTests.Constants;

public class UserRoutes
{
    public const string UserBase = "users-manager/user";
    public static string UserById(Guid id) => $"users-manager/user/{id}";
}