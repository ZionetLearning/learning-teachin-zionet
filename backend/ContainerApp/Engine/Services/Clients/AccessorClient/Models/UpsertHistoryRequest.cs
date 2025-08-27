using System.Text.Json;

namespace Engine.Services.Clients.AccessorClient.Models;

public sealed record UpsertHistoryRequest
{
    public required Guid ThreadId { get; init; }
    public required Guid UserId { get; init; }
    public string? Name { get; set; }

    public string? ChatType { get; init; }
    public required JsonElement History { get; init; }
}
