using Engine.Models.Chat;
using Engine.Models.QueueMessages;
using Engine.Models.Sentences;
using Engine.Models.Words;

namespace Engine.Services;

public interface IAiReplyPublisher
{
    Task SendReplyAsync(UserContextMetadata chatMetadata, EngineChatResponse response, CancellationToken ct = default);
    Task SendGeneratedMessagesAsync(string userId, SentenceResponse response, MessageAction action, CancellationToken ct = default);
    Task SendStreamAsync(UserContextMetadata chatMetadata, EngineChatStreamResponse chunk, CancellationToken ct = default);
    Task SendExplainMessageAsync(string userId, WordExplainResponseDto response, MessageAction action, CancellationToken ct = default);
}