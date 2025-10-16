using System.Text.Json.Serialization;

namespace IntegrationTests.Models.Games;

public class SubmitAttemptResponse
{
    [JsonPropertyName("studentId")]
    public Guid StudentId { get; set; }
    
    [JsonPropertyName("gameType")]
    public string GameType { get; set; } = string.Empty;
    
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("correctAnswer")]
    public List<string> CorrectAnswer { get; set; } = new();
    
    [JsonPropertyName("attemptNumber")]
    public int AttemptNumber { get; set; }
}