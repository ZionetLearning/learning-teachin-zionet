using Engine.Models.Speech;

namespace Engine.Services;

public interface ISpeechSynthesisService
{
    Task<SpeechResponse> SynthesizeAsync(SpeechRequestDto request, CancellationToken cancellationToken = default);

}
