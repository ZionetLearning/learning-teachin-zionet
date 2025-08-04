namespace Engine.Services;

public sealed class SystemPromptProvider : ISystemPromptProvider
{
    public string Prompt { get; }

    public SystemPromptProvider(IConfiguration cfg)
    {

        Prompt = cfg["SystemPrompt"]
              ?? "You are a helpful assistant. Maintain context.";
    }
}