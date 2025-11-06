namespace Engine.Models.Prompts;

public sealed record PromptResponse
{
    public required string PromptKey { get; init; }
    public required string Content { get; init; }
    public int? Version { get; init; }
    public string[]? Labels { get; init; }
    public string[]? Tags { get; init; }
    public string? Type { get; init; }
    public string? Source { get; init; } // "Langfuse" or "Local"
}