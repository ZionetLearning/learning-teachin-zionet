using System.ComponentModel.DataAnnotations;

namespace Manager.Models
{
    public sealed class AiResponseModel
    {
        [Required(ErrorMessage = "Id is required.")]
        [MinLength(1, ErrorMessage = "Id cannot be empty.")]
        public string Id { get; init; } = string.Empty;

        [Required(ErrorMessage = "Answer is required.")]
        [MinLength(1, ErrorMessage = "Answer cannot be empty.")]
        public string Answer { get; init; } = string.Empty;

        public long AnsweredAtUnix { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [Required(ErrorMessage = "Status is required.")]
        [MinLength(1, ErrorMessage = "Status cannot be empty.")]
        public string Status { get; init; } = "ok";
        public string? Error { get; init; }

    }
}