using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotQueue;
using Engine.Models.Sentences;

namespace Engine.Services;
public class ClaudeSentenceGeneratorService : ISentencesService
{
    private readonly ILogger<ClaudeSentenceGeneratorService> _log;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _systemPrompt;

    public ClaudeSentenceGeneratorService(ILogger<ClaudeSentenceGeneratorService> log, IConfiguration config)
    {
        _log = log;
        _httpClient = new HttpClient();
        _apiKey = config["Claude:ApiKey"] ?? throw new Exception();
        var promptPath = Path.Combine(AppContext.BaseDirectory, "Plugins", "Sentences", "Generate", "skprompt.txt");

        _systemPrompt = File.ReadAllText(promptPath, Encoding.UTF8);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<SentenceResponse> GenerateAsync(SentenceRequest req, List<string> userInterests, CancellationToken ct = default)
    {
        var difficulty = req.Difficulty.ToString().ToLowerInvariant();
        var nikud = req.Nikud.ToString().ToLowerInvariant();
        var count = req.Count.ToString(CultureInfo.InvariantCulture);

        var hints = GetRandomHints(difficulty, 3);
        var hintsStr = string.Join(", ", hints);

        //var interest = (userInterests != null && userInterests.Count > 0 && Random.Shared.NextDouble() < 0.5)
        //    ? userInterests[Random.Shared.Next(userInterests.Count)]
        //    : "";

        var interest = (userInterests != null && userInterests.Count > 0)
            ? userInterests[Random.Shared.Next(userInterests.Count)]
            : "";

        var userPrompt = $"Now generate {count} sentences with: difficulty = {difficulty}, nikud = {nikud}, interest = {interest}, hints = {hintsStr}, count = {count}";

        var payload = new
        {
            model = "anthropic/claude-3-haiku",
            messages = new[]
            {
                new { role = "system", content = _systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.3,
            max_tokens = 1024
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            _log.LogError("Claude error: {StatusCode} - {Message}", response.StatusCode, error);
            throw new RetryableException("Claude 3 Haiku call failed.");
        }

        var result = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(result);
        var json = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        if (string.IsNullOrWhiteSpace(json))
        {
            _log.LogError("Claude returned empty JSON.");
            throw new RetryableException("Claude returned empty result.");
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<SentenceResponse>(json);
            if (parsed == null)
            {
                throw new JsonException("Deserialized object is null.");
            }

            return parsed;
        }
        catch (JsonException ex)
        {
            _log.LogError(ex, "Invalid JSON from Claude:\n{Json}", json);
            throw new RetryableException("Claude returned invalid JSON.");
        }
    }

    private string[] GetRandomHints(string difficulty, int count)
    {
        var path = difficulty switch
        {
            "easy" => "Constants/Words/hintsEasy.txt",
            "medium" => "Constants/Words/hintsMedium.txt",
            "hard" => "Constants/Words/hintsHard.txt",
            _ => null
        };

        if (path == null || !File.Exists(path))
        {
            return Array.Empty<string>();
        }

        var pool = File.ReadAllLines(path, Encoding.UTF8)
                       .Select(line => line.Trim())
                       .Where(line => !string.IsNullOrWhiteSpace(line))
                       .Distinct()
                       .ToArray();

        return pool.OrderBy(_ => RandomNumberGenerator.GetInt32(10000)).Take(count).ToArray();
    }
}
