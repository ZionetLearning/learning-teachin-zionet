namespace Engine.Models.Prompts;

public sealed record PromptResponse
{

    public required string PromptKey { get; init; }
    public required string Content { get; init; }
}