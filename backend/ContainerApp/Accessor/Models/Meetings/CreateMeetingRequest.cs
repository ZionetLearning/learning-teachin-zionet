namespace Accessor.Models.Meetings;

public sealed record CreateMeetingRequest
{
    public required List<MeetingAttendee> Attendees { get; set; }
    public DateTimeOffset StartTimeUtc { get; set; }
    public required int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public required Guid CreatedByUserId { get; set; }
}
