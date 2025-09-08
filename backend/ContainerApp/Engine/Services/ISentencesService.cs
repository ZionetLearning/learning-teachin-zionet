using Engine.Models.Sentences;

namespace Engine.Services;

public interface ISentencesService
{
    Task<SentenceResponse> GenerateAsync(SentenceRequest req, CancellationToken ct = default);
}
