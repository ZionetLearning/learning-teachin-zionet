using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accessor.Models.Meetings;

[Table("Meetings")]
public class MeetingModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column(TypeName = "jsonb")]
    public required List<MeetingAttendee> Attendees { get; set; }

    [Required]
    public DateTimeOffset StartTimeUtc { get; set; }

    [Required]
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;

    [Required]
    public Guid GroupCallId { get; set; }

    [Required]
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    public Guid CreatedByUserId { get; set; }
}
