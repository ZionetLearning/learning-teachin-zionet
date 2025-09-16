using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accessor.Models.Games;

public class GameAttempt
{
    [Key]
    public Guid AttemptId { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public string GameType { get; set; } = string.Empty; // "wordOrderGame", etc.

    [Required]
    public Difficulty Difficulty { get; set; }

    [Column(TypeName = "jsonb")]
    [Required]
    public List<string> CorrectAnswer { get; set; } = new();

    [Column(TypeName = "jsonb")]
    [Required]
    public List<string> GivenAnswer { get; set; } = new();

    [Required]
    public bool IsSuccess { get; set; }

    [Required]
    public int AttemptNumber { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}