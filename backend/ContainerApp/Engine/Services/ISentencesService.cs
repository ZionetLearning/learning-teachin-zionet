﻿using Engine.Models.Sentences;

namespace Engine.Services;

public interface ISentencesService
{
    Task<SentenceResponse> GenerateAsync(SentenceRequest req, List<string> userInterests, CancellationToken ct = default);
}
