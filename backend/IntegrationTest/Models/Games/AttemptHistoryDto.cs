namespace IntegrationTests.Models.Games;

public class AttemptHistoryDto
{
    public Guid AttemptId { get; set; }
    public required string GameType { get; set; } = string.Empty;
    public required string Difficulty { get; set; }
    public List<string> GivenAnswer { get; set; } = new();
    public List<string> CorrectAnswer { get; set; } = new();
    public required string Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
