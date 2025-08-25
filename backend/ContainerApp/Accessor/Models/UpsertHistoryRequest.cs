using System.Text.Json;

namespace Accessor.Models;

public sealed record UpsertHistoryRequest
{
    public required Guid ThreadId { get; init; }
    public required string UserId { get; init; }
    public string? ChatType { get; init; } = "default";
    public required JsonElement History { get; init; }
}
