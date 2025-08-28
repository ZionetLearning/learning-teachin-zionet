using System.ComponentModel.DataAnnotations;
using Manager.Models.Chat;

namespace Manager.Services.Clients.Engine.Models;

public sealed record EngineChatRequest
{
    public string RequestId { get; init; } = Guid.NewGuid().ToString("N");

    public required Guid ThreadId { get; init; }

    public required Guid UserId { get; init; }

    [Required(ErrorMessage = "Question is required.")]
    [MinLength(1, ErrorMessage = "Question must be at least 1 character.")]
    public string UserMessage { get; init; } = string.Empty;

    public required ChatType ChatType { get; init; } = ChatType.Default;

    public long SentAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    [Range(1, 172800, ErrorMessage = "TtlSeconds must be between 1 and 172800 (max two day).")]
    public int TtlSeconds { get; init; } = 60;
}