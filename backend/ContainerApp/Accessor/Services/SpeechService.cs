namespace Accessor.Services;

public class SpeechService : ISpeechService
{
    private readonly ILogger<SpeechService> _logger;
    private readonly IConfiguration _configuration;

    public SpeechService(ILogger<SpeechService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> GetSpeechTokenAsync(CancellationToken ct = default)
    {
        try
        {
            var region = _configuration["Speech:Region"];
            var key = _configuration["Speech:Key"];

            if (string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Speech credentials missing (Speech:Region/SPEECH_REGION or Speech:Key/SPEECH_KEY)");
                throw new InvalidOperationException("Speech service not configured");
            }

            using var http = new HttpClient { BaseAddress = new Uri($"https://{region}.api.cognitive.microsoft.com/") };
            http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            using var resp = await http.PostAsync("sts/v1.0/issueToken", content: null, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Speech token request failed with {Status}", resp.StatusCode);
                var body = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Speech token request failed: {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
            }

            var token = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogInformation("Issued speech token for region {Region}", region);
            return token;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Speech service configuration error");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error retrieving speech token");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error retrieving speech token");
            throw;
        }
    }
}