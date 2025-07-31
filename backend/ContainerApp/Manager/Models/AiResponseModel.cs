namespace Manager.Models
{
    public sealed class AiResponseModel
    {
        public string Id { get; init; } = string.Empty;
        public required string ThreadId { get; init; }

        public string Answer { get; init; } = string.Empty;

        public long AnsweredAtUnix { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public string Status { get; init; } = "ok";
        public string? Error { get; init; }

    }
}