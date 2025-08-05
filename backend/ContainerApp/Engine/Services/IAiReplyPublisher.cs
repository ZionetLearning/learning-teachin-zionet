using Engine.Models;

namespace Engine.Services;

public interface IAiReplyPublisher
{
    Task SendReplyAsync(AiResponseModel response, string replyToTopic, CancellationToken ct = default);
}