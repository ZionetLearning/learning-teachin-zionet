using Engine.Models.Sentences;

namespace Engine.Services;

public interface ISentencesService
{
    Task<string> GenerateAsync(SentenceRequest req, CancellationToken ct = default);
}
