namespace IntegrationTests.Constants;

public static class MeetingRoutes
{
    private const string Base = "/meetings-manager";

    public static string GetMeeting(Guid meetingId) => $"{Base}/{meetingId}";
    public static string GetMeetingsForUser(Guid userId) => $"{Base}/user/{userId}";
    public static string CreateMeeting => Base;
    public static string GenerateToken(Guid meetingId) => $"{Base}/{meetingId}/token";
    public static string UpdateMeeting(Guid meetingId) => $"{Base}/{meetingId}";
    public static string DeleteMeeting(Guid meetingId) => $"{Base}/{meetingId}";
}
