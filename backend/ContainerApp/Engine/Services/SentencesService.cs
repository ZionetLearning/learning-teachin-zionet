using System.Security.Cryptography;
using System.Text.Json;
using DotQueue;
using Engine.Models.Sentences;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Engine.Services;

public class SentencesService : ISentencesService
{
    private readonly Kernel _genKernel;
    private readonly ILogger<SentencesService> _log;

    private static readonly string ContentRoot = AppContext.BaseDirectory;
    private static readonly string EasyPath = Path.Combine(ContentRoot, "Constants", "Words", "hintsEasy.txt");
    private static readonly string MediumPath = Path.Combine(ContentRoot, "Constants", "Words", "hintsMedium.txt");
    private static readonly string HardPath = Path.Combine(ContentRoot, "Constants", "Words", "hintsHard.txt");

    private readonly Lazy<string[]> _easyWords;
    private readonly Lazy<string[]> _mediumWords;
    private readonly Lazy<string[]> _hardWords;

    public SentencesService([FromKeyedServices("gen")] Kernel genKernel, ILogger<SentencesService> log)
    {
        _genKernel = genKernel;
        _log = log;

        _easyWords = new(() => LoadList(EasyPath));
        _mediumWords = new(() => LoadList(MediumPath));
        _hardWords = new(() => LoadList(HardPath));
    }

    public async Task<GeneratedSentences> GenerateAsync(SentenceRequest req, List<string> userInterests, CancellationToken ct = default)
    {
        _log.LogInformation("Inside sentence generator service for GameType={GameType}", req.GameType);

        var func = _genKernel.Plugins["Sentences"]["Generate"];

        var exec = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
            ResponseFormat = typeof(GeneratedSentences)
        };

        var difficulty = req.Difficulty.ToString().ToLowerInvariant();
        var hints = GetRandomHints(difficulty, 3);
        var hintsStr = string.Join(", ", hints);

        var args = new KernelArguments(exec)
        {
            ["difficulty"] = difficulty,
            ["nikud"] = req.Nikud.ToString().ToLowerInvariant(),
            ["count"] = req.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["hints"] = hintsStr
        };

        // Add user interest with 50% probability
        if (userInterests != null && userInterests.Count > 0 && Random.Shared.NextDouble() < 0.5)
        {
            var selectedInterest = userInterests[Random.Shared.Next(userInterests.Count)];
            args["interest"] = selectedInterest;
            _log.LogInformation("Injecting user interest into sentence generation: {Interest}", selectedInterest);
        }
        else
        {
            // Ensure no stale variable exists
            args["interest"] = string.Empty;
        }

        var result = await _genKernel.InvokeAsync(func, args, ct);
        var json = result.GetValue<string>();

        if (json is null)
        {
            _log.LogError("Error while generating sentences. The response is empty");
            throw new RetryableException("Error while generating sentences. The response is empty");
        }

        var parsed = JsonSerializer.Deserialize<GeneratedSentences>(json);
        if (parsed is null)
        {
            _log.LogError("Error while generating sentences. The response is empty or invalid JSON");
            throw new RetryableException("Error while generating sentences. The response is empty or invalid JSON");
        }

        // Set GameType from request on all generated sentences
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