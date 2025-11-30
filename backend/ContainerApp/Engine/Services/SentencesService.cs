using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using DotQueue;
using Engine.Constants.Chat;
using Engine.Models;
using Engine.Models.Sentences;
using Engine.Services.Clients.AccessorClient;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Engine.Services;

public class SentencesService : ISentencesService
{
    private readonly IChatClient _chatClient;
    private readonly AzureOpenAIClient _azureClient;
    private readonly AzureOpenAiSettings _cfg;
    private readonly IAccessorClient _accessorClient;
    private readonly ILogger<SentencesService> _log;

    private static readonly string ContentRoot = AppContext.BaseDirectory;
    private static readonly string EasyPath = Path.Combine(ContentRoot, "Constants", "Words", "hintsEasy.txt");
    private static readonly string MediumPath = Path.Combine(ContentRoot, "Constants", "Words", "hintsMedium.txt");
    private static readonly string HardPath = Path.Combine(ContentRoot, "Constants", "Words", "hintsHard.txt");

    private readonly Lazy<string[]> _easyWords;
    private readonly Lazy<string[]> _mediumWords;
    private readonly Lazy<string[]> _hardWords;

    public SentencesService(
        AzureOpenAIClient azureClient,
        IOptions<AzureOpenAiSettings> options,
        IAccessorClient accessorClient,
        ILogger<SentencesService> log)
    {
        _azureClient = azureClient ?? throw new ArgumentNullException(nameof(azureClient));
        _cfg = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _accessorClient = accessorClient ?? throw new ArgumentNullException(nameof(accessorClient));
        _log = log;

        _easyWords = new(() => LoadList(EasyPath));
        _mediumWords = new(() => LoadList(MediumPath));
        _hardWords = new(() => LoadList(HardPath));

        _chatClient = _azureClient.GetChatClient(_cfg.DeploymentName).AsIChatClient();

    }

    public async Task<GeneratedSentences> GenerateAsync(
        SentenceRequest req,
        List<string> userInterests,
        CancellationToken ct = default)
    {
        _log.LogInformation("Inside sentence generator service for GameType={GameType}", req.GameType);

        var difficulty = req.Difficulty.ToString().ToLowerInvariant();
        var hints = GetRandomHints(difficulty, 3);

        var interest = string.Empty;

        if (userInterests is { Count: > 0 } && Random.Shared.NextDouble() < 0.5)
        {
            interest = userInterests[Random.Shared.Next(userInterests.Count)];
            _log.LogInformation("Injecting user interest into sentence generation: {Interest}", interest);
        }

        var promptCfg = await _accessorClient.GetPromptAsync(PromptsKeys.SentencesGenerateTemplate, ct);
        var systemPrompt = promptCfg?.Content;

        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            _log.LogError(
                "Sentences generate template not found or empty for key {Key}, label {Label}",
                PromptsKeys.SentencesGenerateTemplate.Key,
                PromptsKeys.SentencesGenerateTemplate.Label);

            throw new NonRetryableException(
                $"Sentences generation prompt '{PromptsKeys.SentencesGenerateTemplate.Key}' is not configured.");
        }

        var requestPayload = new
        {
            difficulty,
            nikud = req.Nikud,
            count = req.Count,
            hints,
            interest
        };

        var userMessage = JsonSerializer.Serialize(requestPayload);

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var responseFormat = ChatResponseFormat.ForJsonSchema<GeneratedSentences>(jsonSerializerOptions);

        var runOptions = new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions
            {
                Temperature = 0.3f,
                ResponseFormat = responseFormat
            }
        };

        var agent = _chatClient.CreateAIAgent(
            instructions: systemPrompt,
            name: "GenerateSentences");

        var agentResult = await agent.RunAsync(userMessage, thread: null, runOptions, ct);
        var answer = agentResult.Text?.Trim();

        if (string.IsNullOrWhiteSpace(answer))
        {
            _log.LogError("Error while generating sentences. The response is empty");
            throw new RetryableException("Error while generating sentences. The response is empty");
        }

        GeneratedSentences? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<GeneratedSentences>(answer, jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error while deserializing GeneratedSentences. Answer: {Answer}", answer);
            throw new RetryableException("Error while generating sentences. The response is invalid JSON");
        }

        if (parsed is null || parsed.Sentences is null)
        {
            _log.LogError("GeneratedSentences is null or has no sentences. Answer: {Answer}", answer);
            throw new RetryableException("Error while generating sentences. Parsed result is empty");
        }

        var gameType = req.GameType.ToString();
        foreach (var sentence in parsed.Sentences)
        {
            sentence.GameType = gameType;
        }

        return parsed;
    }

    private string[] GetRandomHints(string difficulty, int count)
    {
        var pool = difficulty switch
        {
            "easy" => _easyWords.Value,
            "medium" => _mediumWords.Value,
            "hard" => _hardWords.Value,
            _ => Array.Empty<string>()
        };

        if (pool.Length == 0 || count <= 0)
        {
            return Array.Empty<string>();
        }

        count = Math.Min(count, pool.Length);

        var result = new string[count];
        var indices = Enumerable.Range(0, pool.Length).ToArray();
        for (var i = 0; i < count; i++)
        {
            var j = RandomNumberGenerator.GetInt32(i, pool.Length);
            (indices[i], indices[j]) = (indices[j], indices[i]);
            result[i] = pool[indices[i]];
        }

        return result;
    }

    private string[] LoadList(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                _log.LogWarning("Hints file not found: {Path}", path);
                return Array.Empty<string>();
            }

            return File.ReadAllLines(path, System.Text.Encoding.UTF8)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Exception while reading hints file: {Path}", path);
            return Array.Empty<string>();
        }
    }
}