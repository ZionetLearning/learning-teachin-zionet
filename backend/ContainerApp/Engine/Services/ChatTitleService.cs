using System.Text.Json;
using Azure.AI.OpenAI;
using Engine.Constants.Chat;
using Engine.Models;
using Engine.Services.Clients.AccessorClient;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Engine.Services;

public sealed class ChatTitleService : IChatTitleService
{
    private readonly AzureOpenAIClient _azureClient;
    private readonly AzureOpenAiSettings _cfg;
    private readonly IAccessorClient _accessorClient;
    private readonly IChatClient _chatClient;

    private const int TitleMaxLen = 64;

    public ChatTitleService(
        AzureOpenAIClient azureClient,
        IOptions<AzureOpenAiSettings> options,
        IAccessorClient accessorClient)
    {
        _azureClient = azureClient ?? throw new ArgumentNullException(nameof(azureClient));
        _cfg = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _accessorClient = accessorClient ?? throw new ArgumentNullException(nameof(accessorClient));

        _chatClient = _azureClient.GetChatClient(_cfg.DeploymentName).AsIChatClient();
    }

    public async Task<string> GenerateTitleAsync(string userMessage, CancellationToken ct = default)
    {

        var prompt = await _accessorClient.GetPromptAsync(PromptsKeys.ChatTitlePrompt, ct)
            ?? throw new InvalidOperationException("Chat title prompt not found");

        var system = prompt.Content ?? throw new InvalidOperationException("Prompt content is null");

        var agent = _chatClient.CreateAIAgent(
            instructions: system,
            name: "ChatTitleAgent");

        var thread = agent.GetNewThread();

        var runOptions = new ChatClientAgentRunOptions(new ChatOptions { Temperature = 0f });
        var ar = await agent.RunAsync(userMessage.Trim(), thread, runOptions, ct);
        var raw = ar.Text?.Trim() ?? string.Empty;

        var title = TryParseJsonTitle(raw);
        if (string.IsNullOrWhiteSpace(title))
        {
            title = FallbackTitle(userMessage);
        }

        return PostprocessTitle(title!);
    }

    private static string? TryParseJsonTitle(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
            {
                return t.GetString();
            }
        }
        catch { }

        return null;
    }

    private static string FallbackTitle(string userMessage)
    {
        var changeSymbols = new[] { '.', '?', '!', '\n' };
        var cut = userMessage.Split(changeSymbols, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? userMessage;
        return cut;
    }

    private static string PostprocessTitle(string s)
    {
        s = s.Trim();

        var cleaned = new string(s.Where(ch =>
            !char.IsControl(ch) &&
            ch != '"' && ch != '«' && ch != '»' &&
            ch != '#' && ch != '[' && ch != ']' &&
            ch != '{' && ch != '}' &&
            ch != '|' && ch != '/' && ch != '\\').ToArray());

        if (cleaned.Length > TitleMaxLen)
        {
            cleaned = cleaned[..TitleMaxLen].TrimEnd();

        }

        if (cleaned.Length > 0)
        {
            cleaned = char.ToUpper(cleaned[0]) + cleaned[1..];

        }

        return string.IsNullOrWhiteSpace(cleaned) ? "New chat" : cleaned;
    }
}