using Engine.Models.Chat;
using Engine.Models.QueueMessages;

namespace Engine.Services;

public interface IAiReplyPublisher
{
    Task SendReplyAsync(UserContextMetadata chatMetadata, EngineChatResponse response, CancellationToken ct = default);
}