namespace Engine.Models
{
    public sealed record AiResponseModel
    {
        public string Id { get; init; } = string.Empty;
        public required string ThreadId { get; init; }
        public string Answer { get; init; } = string.Empty;
        public string Status { get; init; } = "ok";
        public string? Error { get; init; }

    }
}
