using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Manager.Models.Games;

/// <summary>
/// Request model from frontend for submitting a game attempt
/// </summary>
public sealed record SubmitAttemptRequest
{
    [Required(ErrorMessage = "ExerciseId is required")]
    [JsonPropertyName("exerciseId")]
    public required Guid ExerciseId { get; init; }

    [Required(ErrorMessage = "GivenAnswer is required")]
    [MinLength(1, ErrorMessage = "GivenAnswer must contain at least one item")]
    [MaxLength(20, ErrorMessage = "GivenAnswer cannot contain more than 20 items")]
    [JsonPropertyName("givenAnswer")]
    public required IReadOnlyList<string> GivenAnswer { get; init; }
}
