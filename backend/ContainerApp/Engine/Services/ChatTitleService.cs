using System.Text.Json;
using Azure.AI.OpenAI;
using Engine.Constants.Chat;
using Engine.Models;
using Engine.Services.Clients.AccessorClient;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Engine.Services;

public sealed class ChatTitleService : IChatTitleService
{
    private readonly AzureOpenAIClient _azureClient;
    private readonly AzureOpenAiSettings _cfg;
    private readonly IAccessorClient _accessorClient;

    private const int TailMessages = 6;
    private const int TitleMaxLen = 64;

    public ChatTitleService(
        AzureOpenAIClient azureClient,
        IOptions<AzureOpenAiSettings> options,
        IAccessorClient accessorClient)
    {
        _azureClient = azureClient ?? throw new ArgumentNullException(nameof(azureClient));
        _cfg = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _accessorClient = accessorClient ?? throw new ArgumentNullException(nameof(accessorClient));
    }

    public async Task<string> GenerateTitleAsync(ChatHistory history, CancellationToken ct = default)
    {
        var tail = new ChatHistory();
        var onlyUser = history
            .Where(m => m.Role == AuthorRole.User && !string.IsNullOrWhiteSpace(m.Content))
            .Reverse()
            .Take(TailMessages)
            .Reverse();

        foreach (var m in onlyUser)
        {
            tail.Add(new ChatMessageContent(AuthorRole.User, m.Content!.Trim()));
        }

        var prompt = await _accessorClient.GetPromptAsync(PromptsKeys.ChatTitlePrompt, ct)
            ?? throw new InvalidOperationException("Chat title prompt not found");

        var system = prompt.Content ?? throw new InvalidOperationException("Prompt content is null");

        var chatClient = _azureClient.GetChatClient(_cfg.DeploymentName).AsIChatClient();

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, system)
        };

        foreach (var m in tail)
        {
            if (!string.IsNullOrWhiteSpace(m.Content))
            {
                messages.Add(new ChatMessage(ChatRole.User, m.Content!));
            }
        }

        var options = new ChatOptions { Temperature = 0 };

        var resp = await chatClient.GetResponseAsync(messages, options, ct);
        var raw = resp.Text?.Trim() ?? string.Empty;

        var title = TryParseJsonTitle(raw);
        if (string.IsNullOrWhiteSpace(title))
        {
            title = FallbackTitle(tail);
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

    private static string FallbackTitle(ChatHistory tail)
    {
        var txt = tail.LastOrDefault(m => m.Role == AuthorRole.User)?.Content
                  ?? tail.LastOrDefault()?.Content
                  ?? "New chat";
        var changeSymbols = new[] { '.', '?', '!', '\n' };
        var cut = txt.Split(changeSymbols, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? txt;
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