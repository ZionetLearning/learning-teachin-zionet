namespace Accessor.Models.Meetings;

public class MeetingDto
{
    public Guid Id { get; set; }
    public required List<MeetingAttendee> Attendees { get; set; }
    public DateTimeOffset StartTimeUtc { get; set; }
    public MeetingStatus Status { get; set; }
    public Guid GroupCallId { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public Guid CreatedByUserId { get; set; }
}
