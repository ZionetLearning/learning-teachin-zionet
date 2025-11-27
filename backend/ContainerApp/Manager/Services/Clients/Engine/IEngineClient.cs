using Manager.Models;
using Manager.Models.Sentences;
using Manager.Models.Words;
using Manager.Services.Clients.Engine.Models;

namespace Manager.Services.Clients.Engine;

public interface IEngineClient
{
    Task<(bool success, string message)> ProcessTaskLongAsync(TaskModel task);
    Task<(bool success, string message)> ChatAsync(EngineChatRequest request);
    Task<(bool success, string message)> GlobalChatAsync(EngineChatRequest request);
    Task<(bool success, string message)> ExplainMistakeAsync(EngineExplainMistakeRequest request);
    Task<GetChatHistoryResponse?> GetHistoryChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default);
    Task<(bool success, string message)> GenerateSentenceAsync(SentenceRequest request);
    Task<(bool success, string message)> GenerateSplitSentenceAsync(SentenceRequest request);
    Task<(bool success, string message)> GenerateWordExplainAsync(WordExplainEngineRequest request, CancellationToken ct = default);
}
