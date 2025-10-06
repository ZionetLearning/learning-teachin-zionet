namespace Engine.Options;

public sealed class PromptKeyOptions
{
    public string? ChatTitlePrompt { get; set; }
    public string? SystemDefault { get; set; }
    public string? FriendlyTone { get; set; }
    public string? DetailedExplanation { get; set; }
    public string? ExplainMistakeSystem { get; set; }
    public string? MistakeTemplate { get; set; }
}