namespace Engine.Models.Prompts;

public sealed record CreatePromptRequest
{
    public required string PromptKey { get; init; }
    public required string Content { get; init; }
}