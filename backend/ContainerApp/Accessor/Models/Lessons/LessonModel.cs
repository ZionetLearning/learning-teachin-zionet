namespace Accessor.Models.Lessons;

public class LessonModel
{
    public Guid LessonId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string ContentSectionsJson { get; set; }
    public Guid TeacherId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}
