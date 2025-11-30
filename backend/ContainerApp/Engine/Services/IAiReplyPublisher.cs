//using Dapr.Client.Autogen.Grpc.v1;
using Engine.Models.Chat;
using Engine.Models.Emails;
using Engine.Models.QueueMessages;
using Engine.Models.Sentences;
using Engine.Models.Words;

namespace Engine.Services;

public interface IAiReplyPublisher
{
    Task SendReplyAsync(UserContextMetadata chatMetadata, EngineChatResponse response, CancellationToken ct = default);
    Task SendGeneratedMessagesAsync(string userId, SentencesResponse response, MessageAction action, CancellationToken ct = default);
    Task SendStreamAsync(UserContextMetadata chatMetadata, EngineChatStreamResponse chunk, CancellationToken ct = default);
    Task SendExplainMessageAsync(string userId, WordExplainResponseDto response, MessageAction action, CancellationToken ct = default);
    Task CreateEmailDraftAsync(string userId, EmailDraftResponse response, MessageAction action, CancellationToken ct = default);
}