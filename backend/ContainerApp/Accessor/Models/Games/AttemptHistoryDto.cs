namespace Accessor.Models.Games;

public class AttemptHistoryDto
{
    public required Guid AttemptId { get; set; }
    public required string GameType { get; set; } = string.Empty;
    public required Difficulty Difficulty { get; set; }
    public required List<string> GivenAnswer { get; set; } = new();
    public required List<string> CorrectAnswer { get; set; } = new();
    public required bool IsSuccess { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}
