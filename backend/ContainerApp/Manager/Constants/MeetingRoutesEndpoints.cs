namespace Manager.Constants;

public static class MeetingRoutesEndpoints
{
    private const string BaseRoute = "meetings-accessor";

    public const string Base = BaseRoute;
    public static string ById(Guid meetingId) => $"{BaseRoute}/{meetingId}";
    public static string ForUser(Guid userId) => $"{BaseRoute}/user/{userId}";
    public static string GenerateToken(Guid meetingId, Guid userId) => $"{BaseRoute}/{meetingId}/token?userId={userId}";
    public static string Identity(Guid userId) => $"{BaseRoute}/identity/{userId}";
}
