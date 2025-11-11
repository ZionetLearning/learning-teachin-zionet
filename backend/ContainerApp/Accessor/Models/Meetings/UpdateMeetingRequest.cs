namespace Accessor.Models.Meetings;

public sealed record UpdateMeetingRequest
{
    public List<MeetingAttendee>? Attendees { get; set; }
    public DateTimeOffset? StartTimeUtc { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Description { get; set; }
    public MeetingStatus? Status { get; set; }
}
