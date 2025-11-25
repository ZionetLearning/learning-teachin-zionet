using Engine.Options;

namespace Engine.Constants.Chat;

public static class PromptsKeys
{

    public static class PromptLabels
    {
        public const string Production = "production";
        public const string Test = "test";
    }
    public static PromptConfiguration ChatTitlePrompt { get; private set; } = new() { Key = "chat.title.generate", Label = PromptLabels.Production };
    public static PromptConfiguration SystemDefault { get; private set; } = new() { Key = "prompts.system.default", Label = PromptLabels.Production };
    public static PromptConfiguration FriendlyTone { get; private set; } = new() { Key = "prompts.tone.friendly", Label = PromptLabels.Production };
    public static PromptConfiguration DetailedExplanation { get; private set; } = new() { Key = "prompts.explanation.detailed", Label = PromptLabels.Production };
    public static PromptConfiguration ExplainMistakeSystem { get; private set; } = new() { Key = "chat.system.explain.mistake", Label = PromptLabels.Production };
    public static PromptConfiguration GlobalChatSystemDefault { get; private set; } = new() { Key = "chat.global.system.default", Label = PromptLabels.Production };
    public static PromptConfiguration GlobalChatPageContext { get; private set; } = new() { Key = "chat.global.page.context", Label = PromptLabels.Production };
    public static PromptConfiguration MistakeUserTemplate { get; private set; } = new() { Key = "prompts.mistake.user.template", Label = PromptLabels.Production };
    public static PromptConfiguration MistakeRuleTemplate { get; private set; } = new() { Key = "prompts.mistake.rule.template", Label = PromptLabels.Production };
    public static PromptConfiguration EmailDraftTemplate { get; private set; } = new() { Key = "prompts.email.template", Label = PromptLabels.Test };

    public static void Configure(PromptKeyOptions? options)
    {
        if (options is null)
        {
            return;
        }

        if (options.ChatTitlePrompt is not null)
        {
            ChatTitlePrompt = options.ChatTitlePrompt;
        }

        if (options.SystemDefault is not null)
        {
            SystemDefault = options.SystemDefault;
        }

        if (options.FriendlyTone is not null)
        {
            FriendlyTone = options.FriendlyTone;
        }

        if (options.DetailedExplanation is not null)
        {
            DetailedExplanation = options.DetailedExplanation;
        }

        if (options.ExplainMistakeSystem is not null)
        {
            ExplainMistakeSystem = options.ExplainMistakeSystem;
        }

        if (options.MistakeUserTemplate is not null)
        {
            MistakeUserTemplate = options.MistakeUserTemplate;
        }

        if (options.MistakeRuleTemplate is not null)
        {
            MistakeRuleTemplate = options.MistakeRuleTemplate;
        }

        if (options.GlobalChatSystemDefault is not null)
        {
            GlobalChatSystemDefault = options.GlobalChatSystemDefault;
        }

        if (options.GlobalChatPageContext is not null)
        {
            GlobalChatPageContext = options.GlobalChatPageContext;
        }

        if (options.EmailDraftTemplate is not null)
        {
            EmailDraftTemplate = options.EmailDraftTemplate;
        }
    }
}