namespace Engine.Models.Prompts;

public sealed record GetPromptsBatchRequest
{
    public required List<string> PromptKeys { get; init; }
    public string? Label { get; init; }
}
