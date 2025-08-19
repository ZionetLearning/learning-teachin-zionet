using Engine.Models.Speech;
using Microsoft.Extensions.Options;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Engine.Services;

public class AzureSpeechSynthesisService : ISpeechSynthesisService
{
    private readonly AzureSpeechSettings _options;
    private readonly ILogger<AzureSpeechSynthesisService> _logger;

    public AzureSpeechSynthesisService(
        IOptions<AzureSpeechSettings> options,
        ILogger<AzureSpeechSynthesisService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SpeechResponse> SynthesizeAsync(SpeechRequestDto request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var voice = string.IsNullOrWhiteSpace(request.VoiceName)
                ? _options.DefaultVoice
                : request.VoiceName;

            _logger.LogInformation("Starting speech synthesis for text length: {Length}", request.Text.Length);

            var speechConfig = SpeechConfig.FromSubscription(_options.SubscriptionKey, _options.Region);
            speechConfig.SpeechSynthesisVoiceName = voice;
            speechConfig.SetProperty("SpeechServiceConnection_SynthVoiceVisemeEvent", "true");

            var visemes = new List<VisemeData>();

            using var stream = AudioOutputStream.CreatePullStream();
            using var audioConfig = AudioConfig.FromStreamOutput(stream);
            using var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

            // Event handlers
            synthesizer.VisemeReceived += (sender, e) =>
            {
                var offsetMs = (long)(e.AudioOffset / 10000);
                visemes.Add(new VisemeData
                {
                    VisemeId = (int)e.VisemeId,
                    OffsetMs = offsetMs
                });
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            SpeechSynthesisResult result;
            try
            {
                result = await synthesizer.SpeakTextAsync(request.Text).WaitAsync(cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                var processingDuration = DateTime.UtcNow - startTime;

                if (!cancellationToken.IsCancellationRequested)
                {
                    // Timeout
                    _logger.LogWarning(ex, "Speech synthesis timed out after {TimeoutSeconds}s", _options.TimeoutSeconds);
                    await synthesizer.StopSpeakingAsync();
                    throw new TimeoutException($"Speech synthesis exceeded {_options.TimeoutSeconds} seconds.", ex);
                }
                else
                {
                    // External cancellation
                    _logger.LogInformation("Speech synthesis canceled by caller after {Duration}ms", processingDuration.TotalMilliseconds);
                    await synthesizer.StopSpeakingAsync();
                    throw;
                }
            }

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                var processingDuration = DateTime.UtcNow - startTime;

                return new SpeechResponse
                {
                    AudioData = Convert.ToBase64String(result.AudioData ?? Array.Empty<byte>()),
                    Visemes = visemes.OrderBy(v => v.OffsetMs).ToList(),
                    Metadata = new SpeechMetadata
                    {
                        AudioLength = result.AudioData?.Length ?? 0,
                        ProcessingDuration = processingDuration
                    }
                };
            }
            else
            {
                var errorMessage = result.Reason == ResultReason.Canceled
                    ? $"Synthesis canceled: {SpeechSynthesisCancellationDetails.FromResult(result).ErrorDetails}"
                    : $"Synthesis failed: {result.Reason}";

                _logger.LogError("Speech synthesis failed: {Error}", errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during speech synthesis");
            throw;
        }
    }
}