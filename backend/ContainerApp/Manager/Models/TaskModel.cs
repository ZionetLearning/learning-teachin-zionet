using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Manager.Models;

public class TaskModel
{
    [JsonPropertyName("id")]
    [Range(1, int.MaxValue, ErrorMessage = "Id must be a positive integer.")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(1, ErrorMessage = "Name must be at least 1 character.")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public required string Name { get; set; }

    [JsonPropertyName("payload")]
    [Required(ErrorMessage = "Payload is required.")]
    [MinLength(1, ErrorMessage = "Payload must be at least 1 character.")]
    public required string Payload { get; set; }
}
