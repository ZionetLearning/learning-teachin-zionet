using System.ComponentModel.DataAnnotations;

namespace Manager.Models.Tasks.Requests;

/// <summary>
/// Request model for creating a task
/// </summary>
public sealed record CreateTaskRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Id must be a positive integer.")]
    public int Id { get; init; }

    [Required(ErrorMessage = "Name is required.")]
    [MinLength(1, ErrorMessage = "Name must be at least 1 character.")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public required string Name { get; init; }

    [Required(ErrorMessage = "Payload is required.")]
    [MinLength(1, ErrorMessage = "Payload must be at least 1 character.")]
    public required string Payload { get; init; }
}
