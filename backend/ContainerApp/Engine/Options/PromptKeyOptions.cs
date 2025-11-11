namespace Engine.Options;

public sealed class PromptKeyOptions
{
    public PromptConfiguration? ChatTitlePrompt { get; set; }
    public PromptConfiguration? SystemDefault { get; set; }
    public PromptConfiguration? FriendlyTone { get; set; }
    public PromptConfiguration? DetailedExplanation { get; set; }
    public PromptConfiguration? ExplainMistakeSystem { get; set; }
    public PromptConfiguration? MistakeTemplate { get; set; }
    public PromptConfiguration? GlobalChatSystemDefault { get; set; }
    public PromptConfiguration? GlobalChatPageContext { get; set; }
}