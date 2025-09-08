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

    private const string EasyPath = "Constants/Words/hintsEasy.txt";
    private const string MediumPath = "Constants/Words/hintsMedium.txt";
    private const string HardPath = "Constants/Words/hintsHard.txt";

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

    public async Task<SentenceResponse> GenerateAsync(SentenceRequest req, CancellationToken ct = default)
    {
        _log.LogInformation("Inside sentence generator service");

        var func = _genKernel.Plugins["Sentences"]["Generate"];

        var exec = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
            ResponseFormat = typeof(SentenceResponse)
        };

        var difficulty = req.Difficulty.ToString().ToLowerInvariant();
        var hints = GetRandomHints(difficulty, 3);
        var hintsStr = string.Join(", ", hints);

        var args = new KernelArguments(exec)
        {
            ["difficulty"] = difficulty,
            ["nikud"] = req.Nikud.ToString().ToLowerInvariant(),
            ["count"] = req.Count.ToString(),
            ["hints"] = hintsStr
        };

        var result = await _genKernel.InvokeAsync(func, args, ct);
        var json = result.GetValue<string>();

        if (json is null)
        {
            _log.LogError("Error while generating sentences. The response is empty");
            throw new RetryableException("Error while generating sentences. The response is empty");
        }

        var parsed = JsonSerializer.Deserialize<SentenceResponse>(json);
        if (parsed is null)
        {
            _log.LogError("Error while generating sentences. The response is empty or invalid JSON");
            throw new RetryableException("Error while generating sentences. The response is empty or invalid JSON");
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

            return File.ReadAllLines(path)
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