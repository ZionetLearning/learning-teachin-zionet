namespace IntegrationTests.Models.Games;

/// <summary>
/// DTO model matching the AttemptedSentenceResult from the accessor service.
/// Represents the result of saving generated sentences as pending game attempts.
/// </summary>
public class AttemptedSentenceResult
{
    public Guid AttemptId { get; set; }
    public string Original { get; set; } = string.Empty;
    public List<string> Words { get; set; } = new();
    public string Difficulty { get; set; } = string.Empty;
    public bool Nikud { get; set; }
}