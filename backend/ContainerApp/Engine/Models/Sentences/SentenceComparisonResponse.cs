namespace Engine.Models.Sentences;

public class ModelResult
{
    public string Provider { get; set; } = string.Empty;
    public TimeSpan Latency { get; set; }
    public SentenceResponse? Response { get; set; }
}

public class SentenceComparisonResponse
{
    public List<ModelResult> Results { get; set; } = new();
}
