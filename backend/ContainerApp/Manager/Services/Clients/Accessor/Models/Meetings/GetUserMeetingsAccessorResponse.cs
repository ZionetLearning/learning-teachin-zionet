namespace Manager.Services.Clients.Accessor.Models.Meetings;

/// <summary>
/// Response model received from Accessor service for getting user meetings
/// </summary>
public sealed record GetUserMeetingsAccessorResponse
{
    public required IReadOnlyList<GetMeetingAccessorResponse> Meetings { get; init; }
}
