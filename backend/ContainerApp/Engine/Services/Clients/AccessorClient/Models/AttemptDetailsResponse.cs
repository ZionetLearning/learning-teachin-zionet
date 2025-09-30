namespace Engine.Services.Clients.AccessorClient.Models;

public sealed record AttemptDetailsResponse
{
    public required Guid AttemptId { get; init; }
    public required Guid ExerciseId { get; init; }
    public required List<string> UserAnswer { get; init; }
    public required List<string> CorrectAnswer { get; init; }
    public required string ErrorType { get; init; }
}