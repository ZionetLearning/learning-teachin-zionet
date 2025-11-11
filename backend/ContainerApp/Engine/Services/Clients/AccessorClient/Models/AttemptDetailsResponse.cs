using Engine.Models.Games;
namespace Engine.Services.Clients.AccessorClient.Models;

public sealed record AttemptDetailsResponse
{
    public required Guid AttemptId { get; init; }
    public required GameName GameType { get; init; }
    public required List<string> GivenAnswer { get; init; }
    public required List<string> CorrectAnswer { get; init; }
    public required string Difficulty { get; init; }
}