using Engine.Models;

namespace Engine.Services;

public interface IAiReplyPublisher
{
    Task PublishAsync(AiResponseModel response, string replyToTopic, CancellationToken ct = default);
}