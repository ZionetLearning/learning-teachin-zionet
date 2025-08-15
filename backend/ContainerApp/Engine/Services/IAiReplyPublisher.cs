using Engine.Models.Chat;

namespace Engine.Services;

public interface IAiReplyPublisher
{
    Task SendReplyAsync(EngineChatResponse response, string replyToQueue, CancellationToken ct = default);
}