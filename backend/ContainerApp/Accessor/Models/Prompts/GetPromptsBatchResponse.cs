namespace Accessor.Models.Prompts;

public sealed record GetPromptsBatchResponse
{
    public required List<PromptResponse> Prompts { get; init; }
    public required List<string> NotFound { get; init; }
}