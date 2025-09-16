namespace Accessor.Models.Games;

public class MistakeDto
{
    public required string GameType { get; set; } = string.Empty;
    public required Difficulty Difficulty { get; set; }
    public required List<string> LastWrongAnswer { get; set; } = new();
}