namespace Accessor.Models.Prompts;

public sealed record GetPromptsBatchRequest
{
    public required List<string> PromptKeys { get; init; }
}
