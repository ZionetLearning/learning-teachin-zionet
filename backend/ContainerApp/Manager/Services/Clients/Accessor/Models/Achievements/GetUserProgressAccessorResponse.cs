namespace Manager.Services.Clients.Accessor.Models.Achievements;

public sealed record GetUserProgressAccessorResponse
{
    public required Guid UserProgressId { get; init; }
    public required Guid UserId { get; init; }
    public required string Feature { get; init; }
    public required int Count { get; init; }
    public required DateTime LastUpdated { get; init; }
}
