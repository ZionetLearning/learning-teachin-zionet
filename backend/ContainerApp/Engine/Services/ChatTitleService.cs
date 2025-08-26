using System.Text.Json;
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

    private const int TailMessages = 6;
    private const int TitleMaxLen = 64;

    public ChatTitleService(Kernel kernel, IRetryPolicy retryPolicy, ILogger<ChatTitleService> log)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
        _kernelPolicy = retryPolicy.CreateKernelPolicy(log);
    }

    public async Task<string> GenerateAsync(ChatHistory history, CancellationToken ct = default)
    {
        var tail = new ChatHistory();
        var onlyUser = history
               .Where(m => m.Role == AuthorRole.User && !string.IsNullOrWhiteSpace(m.Content))
               .Reverse()
               .Take(TailMessages) // последние N пользовательских
               .Reverse();

        foreach (var m in onlyUser)
        {
            tail.Add(new ChatMessageContent(AuthorRole.User, m.Content!.Trim()));

        }

        var system = """
You are a naming assistant. Create a short, specific chat title that captures the main topic.

Rules:
- Language: match the user's recent messages language.
- ≤ 6 words, ≤ 50 characters.
- No quotes, emojis, hashtags, brackets, file names, or PII.
- Title case for English; sentence case for Russian/others.
Return STRICT JSON: {"title":"..."}
""";

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
        //todo: think what I need do here
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