namespace Accessor.Models.Meetings;

public sealed record MeetingDto
{
    public Guid Id { get; set; }
    public required List<MeetingAttendee> Attendees { get; set; }
    public DateTimeOffset StartTimeUtc { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public MeetingStatus Status { get; set; }
    public required string GroupCallId { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public Guid CreatedByUserId { get; set; }
}
