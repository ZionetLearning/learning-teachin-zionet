using Engine.Constants;

namespace Engine.Models
{
    public sealed class AiRequestModel
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");

        public string Question { get; init; } = string.Empty;

        public long SentAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public int TtlSeconds { get; init; } = 60;

        public string ReplyToTopic { get; init; } = TopicNames.AiToManager;

    }
}

