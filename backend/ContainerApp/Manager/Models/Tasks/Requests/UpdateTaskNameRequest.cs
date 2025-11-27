using System.ComponentModel.DataAnnotations;

namespace Manager.Models.Tasks.Requests;

/// <summary>
/// Request model for updating a task name
/// </summary>
public sealed record UpdateTaskNameRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(1, ErrorMessage = "Name must be at least 1 character.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public required string Name { get; init; }
}
