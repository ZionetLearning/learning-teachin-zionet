using Engine.Options;

namespace Engine.Constants.Chat;

public static class PromptsKeys
{
    public static string ChatTitlePrompt { get; private set; } = "chat.title.generate";
    public static string SystemDefault { get; private set; } = "prompts.system.default";
    public static string FriendlyTone { get; private set; } = "prompts.tone.friendly";
    public static string DetailedExplanation { get; private set; } = "prompts.explanation.detailed";
    public static string Interests { get; private set; } = "prompts.interests.injection";

    public static void Configure(PromptKeyOptions? options)
    {
        if (options is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(options.ChatTitlePrompt))
        {
            ChatTitlePrompt = options.ChatTitlePrompt;
        }

        if (!string.IsNullOrWhiteSpace(options.SystemDefault))
        {
            SystemDefault = options.SystemDefault;
        }

        if (!string.IsNullOrWhiteSpace(options.FriendlyTone))
        {
            FriendlyTone = options.FriendlyTone;
        }

        if (!string.IsNullOrWhiteSpace(options.DetailedExplanation))
        {
            DetailedExplanation = options.DetailedExplanation;
        }

        if (!string.IsNullOrWhiteSpace(options.Interests))
        {
            Interests = options.Interests;
        }
    }
}