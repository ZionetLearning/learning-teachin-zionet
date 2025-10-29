namespace Accessor.Models.Meetings;

public class UpdateMeetingRequest
{
    public List<MeetingAttendee>? Attendees { get; set; }
    public DateTimeOffset? StartTimeUtc { get; set; }
    public MeetingStatus? Status { get; set; }
    public Guid? GroupCallId { get; set; }
}
