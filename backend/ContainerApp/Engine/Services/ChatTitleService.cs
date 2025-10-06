using System.Text.Json;
using Engine.Constants.Chat;
using Engine.Services.Clients.AccessorClient;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Polly;

namespace Engine.Services;

public sealed class ChatTitleService : IChatTitleService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly IAsyncPolicy<ChatMessageContent> _kernelPolicy;
    private readonly IAccessorClient _accessorClient;

    private const int TailMessages = 6;
    private const int TitleMaxLen = 64;

    public ChatTitleService(Kernel kernel, IRetryPolicy retryPolicy, ILogger<ChatTitleService> log, IAccessorClient accessorClient)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
        _kernelPolicy = retryPolicy.CreateKernelPolicy(log);
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

        var prompt = await _accessorClient.GetPromptAsync(PromptsKeys.ChatTitlePrompt, ct);
        if (prompt is null)
        {
            throw new InvalidOperationException("Chat title prompt not found");
        }

        var system = prompt.Content ?? throw new InvalidOperationException("Prompt content is null");

        var tmp = new ChatHistory();
        tmp.AddSystemMessage(system);
        foreach (var m in tail)
        {
            tmp.Add(m);

        }

        var settings = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var result = await _kernelPolicy.ExecuteAsync(
            ct2 => _chat.GetChatMessageContentAsync(tmp, settings, _kernel, ct2), ct);

        var raw = result?.Content?.Trim() ?? string.Empty;

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
        // TODO: think about what I need to do here
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