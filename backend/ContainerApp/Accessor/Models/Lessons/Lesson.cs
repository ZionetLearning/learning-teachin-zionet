using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accessor.Models.Lessons;

public class Lesson
{
    [Key]
    public Guid LessonId { get; set; }

    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Description { get; set; }

    [Column(TypeName = "jsonb")]
    [Required]
    public List<ContentSection> ContentSections { get; set; } = [];

    [Required]
    public Guid TeacherId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime ModifiedAt { get; set; }
}

