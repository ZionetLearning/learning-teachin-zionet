namespace IntegrationTests.Constants;

public static class GamesRoutes
{
    private const string BaseRoute = "games-manager";

    /// <summary>
    /// POST endpoint to submit a game attempt.
    /// Route: games-manager/attempt
    /// </summary>
    public const string Attempt = $"{BaseRoute}/attempt";

    /// <summary>
    /// GET endpoint to retrieve game history for a specific student.
    /// Route: games-manager/history/{studentId}
    /// </summary>
    /// <param name="studentId">The student's user ID</param>
    /// <returns>The full route with the student ID</returns>
    public static string History(Guid studentId) => $"{BaseRoute}/history/{studentId}";

    /// <summary>
    /// GET endpoint to retrieve mistakes for a specific student.
    /// Route: games-manager/mistakes/{studentId}
    /// </summary>
    /// <param name="studentId">The student's user ID</param>
    /// <returns>The full route with the student ID</returns>
    public static string Mistakes(Guid studentId) => $"{BaseRoute}/mistakes/{studentId}";

    /// <summary>
    /// GET/DELETE endpoint to retrieve or delete all game history.
    /// Route: games-manager/all-history
    /// </summary>
    public const string AllHistory = $"{BaseRoute}/all-history";
}
