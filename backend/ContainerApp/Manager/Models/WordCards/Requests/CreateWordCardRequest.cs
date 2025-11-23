using System.ComponentModel.DataAnnotations;

namespace Manager.Models.WordCards.Requests;

/// <summary>
/// Request model for creating a word card (from frontend)
/// </summary>
public sealed record CreateWordCardRequest
{
    [Required]
    public required string Hebrew { get; init; }

    [Required]
    public required string English { get; init; }

    public string? Explanation { get; init; }
}
