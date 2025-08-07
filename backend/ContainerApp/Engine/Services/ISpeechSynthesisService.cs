using Engine.Models.Speech;

namespace Engine.Services;

public interface ISpeechSynthesisService
{
    Task<SpeechResponse> SynthesizeAsync(SpeechRequest request, CancellationToken cancellationToken = default);

}
