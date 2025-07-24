using Manager.Constants;

namespace Manager.Models
{
    public sealed class AiRequestModel
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");

        public string Question { get; init; } = string.Empty;

        public long SentAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

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
