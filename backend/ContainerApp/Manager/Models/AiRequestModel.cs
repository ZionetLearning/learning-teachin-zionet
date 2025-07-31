using Manager.Constants;
using System.ComponentModel.DataAnnotations;

namespace Manager.Models
{
    public sealed class AiRequestModel
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");

        [Required(ErrorMessage = "Question is required.")]
        [MinLength(1, ErrorMessage = "Question must be at least 1 character.")]
        public string Question { get; init; } = string.Empty;

        public long SentAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [Range(1, 172800, ErrorMessage = "TtlSeconds must be between 1 and 172800 (max two day).")]
        public int TtlSeconds { get; init; } = 60;

        [Required(ErrorMessage = "ReplyToTopic is required.")]
        [MinLength(1, ErrorMessage = "ReplyToTopic cannot be empty.")]
        public string ReplyToTopic { get; init; } = TopicNames.AiToManager;

        public static AiRequestModel Create(string question, string replyToTopic, int ttlSeconds = 60)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var id = Guid.NewGuid().ToString("N");
            return new AiRequestModel
            {
                Id = id,
                Question = question,
                TtlSeconds = ttlSeconds,
                ReplyToTopic = replyToTopic,
                SentAt = now
            };
        }
    }
}
