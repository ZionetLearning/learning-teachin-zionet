using System.Text.Json.Serialization;

namespace IntegrationTests.Models.Games;

public class SubmitAttemptRequest
{
    [JsonPropertyName("studentId")]
    public required Guid StudentId { get; set; }
    
    [JsonPropertyName("attemptId")]
    public Guid AttemptId { get; set; }
    
    [JsonPropertyName("givenAnswer")]
    public required List<string> GivenAnswer { get; set; } = new();
}
