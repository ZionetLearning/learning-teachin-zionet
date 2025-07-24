using Manager.Constants;

namespace Manager.Models
{
    public sealed class AiRequestModel
    {
        public required string Id { get; init; }

        public string Question { get; init; } = string.Empty;

        public long SentAt { get; init; }

        public int TtlSeconds { get; init; } = 60;

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
