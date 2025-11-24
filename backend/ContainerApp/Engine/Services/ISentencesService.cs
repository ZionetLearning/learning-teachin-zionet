using Engine.Models.Sentences;

namespace Engine.Services;

public interface ISentencesService
{
    Task<GeneratedSentences> GenerateAsync(SentenceRequest req, List<string> userInterests, CancellationToken ct = default);
}
