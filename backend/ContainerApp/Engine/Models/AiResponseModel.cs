namespace Engine.Models
{
    public sealed record AiResponseModel
    {
        public string Id { get; init; } = string.Empty;
        public string Answer { get; init; } = string.Empty;

        public long AnsweredAtUnix { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public string Status { get; init; } = "ok";
        public string? Error { get; init; }

    }
}
