using System.Text.Json;

namespace Engine.Services.Clients.AccessorClient.Models;

public sealed record HistorySnapshotDto
{
    public Guid ThreadId { get; init; }
    public required string UserId { get; init; }
    public string ChatType { get; init; } = "default";
    public required JsonElement History { get; init; }
}
