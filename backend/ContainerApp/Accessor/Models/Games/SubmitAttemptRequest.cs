using System.Text.Json.Serialization;

namespace Accessor.Models.Games;

public class SubmitAttemptRequest
{
    public required Guid StudentId { get; set; }
    [JsonPropertyName("attemptId")]
    public Guid ExerciseId { get; set; }
    public required List<string> GivenAnswer { get; set; } = new();
}