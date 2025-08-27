using System.Text.Json;

namespace Engine.Services.Clients.AccessorClient.Models;

public sealed record HistorySnapshotDto
{
    public Guid ThreadId { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ChatType { get; init; } = "default";
    public required JsonElement History { get; init; }
}
