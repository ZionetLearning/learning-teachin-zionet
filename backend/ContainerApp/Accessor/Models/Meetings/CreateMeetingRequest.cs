namespace Accessor.Models.Meetings;

public class CreateMeetingRequest
{
    public required List<MeetingAttendee> Attendees { get; set; }
    public DateTimeOffset StartTimeUtc { get; set; }
    public Guid GroupCallId { get; set; }
    public required Guid CreatedByUserId { get; set; }
}
