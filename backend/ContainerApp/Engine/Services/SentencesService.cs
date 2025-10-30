using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
//using System.Text.Json;
//using DotQueue;
using Engine.Models.Sentences;
using Microsoft.SemanticKernel;
//using OpenAI.Chat;
//using Microsoft.SemanticKernel.ChatCompletion;
//using Microsoft.SemanticKernel.ChatCompletion;
//using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

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
    private readonly JsonSerializerOptions settings = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions _prettyJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping

    };

    //public SentencesService([FromKeyedServices("gen")] Kernel genKernel, ILogger<SentencesService> log)
    public SentencesService(Kernel kernel, ILogger<SentencesService> log)

    {
        _genKernel = kernel;
        _log = log;
        _easyWords = new(() => LoadList(EasyPath));
        _mediumWords = new(() => LoadList(MediumPath));
        _hardWords = new(() => LoadList(HardPath));
    }

    public async Task<SentenceResponse> GenerateAsync(SentenceRequest req, List<string> userInterests, CancellationToken ct = default)
    {
        _log.LogInformation("Inside sentence generator service");

        var difficulty = req.Difficulty.ToString().ToLowerInvariant();
        var hints = GetRandomHints(difficulty, 3);
        var hintsStr = string.Join(", ", hints);

        var args = new KernelArguments
        {
            ["difficulty"] = difficulty,
            ["nikud"] = req.Nikud.ToString().ToLowerInvariant(),
            ["count"] = req.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["hints"] = hintsStr,
            ["interest"] = string.Empty
        };

        // Add user interest with 50% chance
        if (userInterests?.Count > 0 && Random.Shared.NextDouble() < 0.5)
        {
            var interest = userInterests[Random.Shared.Next(userInterests.Count)];
            args["interest"] = interest;
            _log.LogInformation("Injecting interest: {Interest}", interest);
        }

        // Compare across models
        var services = new[] {
            "gpt"
            , "claude"
            ,
            "phi"
        };
        var comparison = new SentenceComparisonResponse();

        foreach (var serviceId in services)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var result = await RunPluginAsync(_genKernel, serviceId, "Sentences", "Generate", args, ct);
                sw.Stop();

                var res = new ModelResult
                {
                    Provider = serviceId,
                    Latency = sw.Elapsed,
                    Response = result
                };

                comparison.Results.Add(res);

                _log.LogInformation("Model: {Provider} | Latency: {Latency}ms | Sentence count: {Count}",
                    res.Provider,
                    res.Latency.TotalMilliseconds,
                    res.Response?.Sentences?.Count ?? 0);

                var index = 1;
                foreach (var s in res.Response!.Sentences!)
                {
                    _log.LogInformation("Sentence {Index}: {Sentence}", index++, s.Text);
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                _log.LogError(ex, "{Provider} failed during sentence generation", serviceId);

                comparison.Results.Add(new ModelResult
                {
                    Provider = serviceId,
                    Latency = sw.Elapsed,
                    Response = null
                });
            }
        }

        SaveComparisonToFile(comparison);

        _log.LogInformation("Completed sentence generation comparison");

        return new SentenceResponse { Sentences = [] };
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

    private async Task<SentenceResponse?> RunPluginAsync(
    Kernel kernel,
    string serviceId,
    string pluginName,
    string functionName,
    KernelArguments args,
    CancellationToken ct = default)
    {
        var execSettings = new PromptExecutionSettings
        {
            ServiceId = serviceId,
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = 0.3,
                ["max_tokens_to_sample"] = 512
            }
        };

        var scopedArgs = new KernelArguments(execSettings);

        foreach (var kvp in args)
        {
            scopedArgs[kvp.Key] = kvp.Value;
        }

        var func = kernel.Plugins[pluginName][functionName];
        var result = await kernel.InvokeAsync(func, scopedArgs, ct);
        var json = result.GetValue<string>();
        if (string.IsNullOrWhiteSpace(json))
        {
            _log.LogWarning("Received empty response from plugin {Plugin}.{Function}", pluginName, functionName);
            return null;
        }

        var sanitizeJson = SanitizeJson(json);

        var parsedResult = JsonSerializer.Deserialize<SentenceResponse>(
            sanitizeJson, settings
        );
        return parsedResult;

    }

    private static string SanitizeJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        raw = raw.Trim();

        // Remove ```json ... ``` or ``` ... ``` Markdown blocks
        if (raw.StartsWith("```"))
        {
            var start = raw.IndexOf('\n');
            var end = raw.LastIndexOf("```");

            if (start >= 0 && end > start)
            {
                raw = raw.Substring(start + 1, end - start - 1).Trim();
            }
        }

        return raw;
    }

    private void SaveComparisonToFile(SentenceComparisonResponse comparison)
    {
        try
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "ModelLogs");
            Directory.CreateDirectory(folder);

            var fileName = $"comparison_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.json";
            var filePath = Path.Combine(folder, fileName);

            var json = JsonSerializer.Serialize(comparison, _prettyJsonOptions);

            File.WriteAllText(filePath, json);

            _log.LogInformation("Saved model comparison to file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to save model comparison to file.");
        }
    }
}