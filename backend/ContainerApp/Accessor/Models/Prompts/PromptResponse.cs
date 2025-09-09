namespace Accessor.Models.Prompts;

public record PromptResponse
{
    public string PromptKey { get; set; } = null!;
    public string Content { get; set; } = null!;
}
