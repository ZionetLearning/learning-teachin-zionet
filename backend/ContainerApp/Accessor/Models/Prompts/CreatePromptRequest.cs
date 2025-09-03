namespace Accessor.Models.Prompts;

public record CreatePromptRequest
{
    public string PromptKey { get; set; } = null!;
    public string Content { get; set; } = null!;
}
