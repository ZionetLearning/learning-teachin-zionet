using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Accessor.Models.GameConfiguration;

namespace Accessor.Models.Games;

public class GameAttempt
{
    // for each attempt to answer
    [Key]
    public Guid AttemptId { get; set; }

    // for each generated exercise/sentence 
    [Required]
    public Guid ExerciseId { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public GameName GameType { get; set; }

    [Required]
    public Difficulty Difficulty { get; set; }

    [Column(TypeName = "jsonb")]
    [Required]
    public List<string> CorrectAnswer { get; set; } = new();

    [Column(TypeName = "jsonb")]
    [Required]
    public List<string> GivenAnswer { get; set; } = new();

    [Required]
    public AttemptStatus Status { get; set; }

    [Required]
    public int AttemptNumber { get; set; }

    [Required]
    public decimal Accuracy { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}