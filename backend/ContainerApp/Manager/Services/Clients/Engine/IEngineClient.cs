using Manager.Models;
using Manager.Models.Sentences;
using Manager.Models.Speech;
using Manager.Services.Clients.Engine.Models;

namespace Manager.Services.Clients.Engine;

public interface IEngineClient
{
    Task<(bool success, string message)> ProcessTaskLongAsync(TaskModel task);
    Task<(bool success, string message)> ChatAsync(EngineChatRequest request);
    Task<ChatHistoryForFrontDto?> GetHistoryChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default);
    Task<SpeechEngineResponse?> SynthesizeAsync(SpeechRequest request, CancellationToken cancellationToken = default);
    Task<(bool success, string message)> GenerateSentenceAsync(SentenceRequest request);
}
