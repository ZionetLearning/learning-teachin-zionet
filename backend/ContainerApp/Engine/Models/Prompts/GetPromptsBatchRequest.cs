using Engine.Options;

namespace Engine.Models.Prompts;

public sealed record GetPromptsBatchRequest
{
    public required List<PromptConfiguration> Prompts { get; init; }
}
