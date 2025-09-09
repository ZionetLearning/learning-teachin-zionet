namespace Accessor.Services;

public class SpeechService : ISpeechService
{
    private readonly ILogger<SpeechService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public SpeechService(ILogger<SpeechService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetSpeechTokenAsync(CancellationToken ct = default)
    {
        try
        {
            var http = _httpClientFactory.CreateClient("SpeechClient");

            using var resp = await http.PostAsync("sts/v1.0/issueToken", content: null, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Speech token request failed with {Status}", resp.StatusCode);
                var body = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Speech token request failed: {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
            }

            var token = await resp.Content.ReadAsStringAsync(ct);
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