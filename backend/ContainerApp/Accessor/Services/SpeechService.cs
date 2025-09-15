using Accessor.Models.Speech;
using Accessor.Services.Interfaces;

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

    public async Task<SpeechTokenResponse> GetSpeechTokenAsync(CancellationToken ct = default)
    {
        try
        {
            var http = _httpClientFactory.CreateClient("SpeechClient");

            using var resp = await http.PostAsync("sts/v1.0/issueToken", content: null, ct); // the key and region are in the client config in Program.cs
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Speech token request failed with {Status}", resp.StatusCode);
                var body = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Speech token request failed: {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
            }

            var token = await resp.Content.ReadAsStringAsync(ct);
            var region = _configuration["Speech:Region"];
            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(region))
            {
                return new SpeechTokenResponse { Token = token, Region = region };
            }

            _logger.LogError("token response or region was empty \n token:'{Token}' \n region:'{Region}'", token, region);
            throw new InvalidOperationException("Speech token response was empty");
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