using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Azure.AI.OpenAI;
using DotQueue;
using Engine.Constants.Chat;
using Engine.Models;
using Engine.Models.Words;
using Engine.Services.Clients.AccessorClient;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Engine.Services;

public interface IWordExplainService
{
    Task<WordExplainResponse> ExplainAsync(WordExplainRequest req, string lang, CancellationToken ct = default);
}

public sealed class WordExplainService : IWordExplainService
{
    private readonly IChatClient _chatClient;
    private readonly AzureOpenAIClient _azureClient;
    private readonly AzureOpenAiSettings _cfg;
    private readonly IAccessorClient _accessorClient;
    private readonly ILogger<WordExplainService> _log;

    public WordExplainService(
        AzureOpenAIClient azureClient,
        IOptions<AzureOpenAiSettings> options,
        IAccessorClient accessorClient,
        ILogger<WordExplainService> log)
    {
        _azureClient = azureClient ?? throw new ArgumentNullException(nameof(azureClient));
        _cfg = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _accessorClient = accessorClient ?? throw new ArgumentNullException(nameof(accessorClient));
        _log = log ?? throw new ArgumentNullException(nameof(log));

        _chatClient = _azureClient.GetChatClient(_cfg.DeploymentName).AsIChatClient();
    }

    public async Task<WordExplainResponse> ExplainAsync(
        WordExplainRequest req,
        string lang,
        CancellationToken ct = default)
    {
        _log.LogInformation("Inside word explain service for word {Word}", req.Word);

        var promptCfg = await _accessorClient.GetPromptAsync(PromptsKeys.WordExplanationTemplate, ct);
        var template = promptCfg?.Content;

        if (string.IsNullOrWhiteSpace(template))
        {
            _log.LogError(
                "Word explain template not found or empty. Key={Key}, Label={Label}",
                PromptsKeys.WordExplanationTemplate.Key,
                PromptsKeys.WordExplanationTemplate.Label);

            throw new NonRetryableException(
                $"Word explain prompt '{PromptsKeys.WordExplanationTemplate.Key}' is not configured.");
        }

        var systemPrompt = template
            .Replace("{{$lang}}", lang, StringComparison.Ordinal)
            .Replace("{{$word}}", req.Word, StringComparison.Ordinal)
            .Replace("{{$context}}", req.Context, StringComparison.Ordinal);

        var payload = new
        {
            word = req.Word,
            context = req.Context,
            lang
        };

        var userMessage = JsonSerializer.Serialize(payload);

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters = { new JsonStringEnumConverter() }
        };

        var agent = _chatClient.CreateAIAgent(
            instructions: systemPrompt,
            name: "WordExplain");

        var responseFormat = ChatResponseFormat.ForJsonSchema<WordExplainResponse>(jsonSerializerOptions);

        var runOptions = new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions
            {
                Temperature = 0.2f,
                ResponseFormat = responseFormat
            }
        };

        var result = await agent.RunAsync(userMessage, thread: null, runOptions, ct);

        var answer = result.Text?.Trim();

        if (string.IsNullOrWhiteSpace(answer))
        {
            _log.LogError("Empty response for word {Word}", req.Word);
            throw new RetryableException($"Empty response for word {req.Word}");
        }

        WordExplainResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<WordExplainResponse>(answer, jsonSerializerOptions);
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                "Error while deserializing WordExplainResponse for word {Word}. Answer: {Answer}",
                req.Word,
                answer);

            throw new RetryableException("Invalid JSON format from model");
        }

        if (parsed is null)
        {
            _log.LogError(
                "Parsed WordExplainResponse is null for word {Word}. Answer: {Answer}",
                req.Word,
                answer);

            throw new RetryableException("Empty parsed WordExplainResponse");
        }

        return parsed;

    }
}
