using Engine.Models.Chat;

namespace Engine.Services;

public interface IChatAiService
{
    Task<ChatAiServiceResponse> ChatHandlerAsync(EngineChatRequest request, CancellationToken ct = default);

    IAsyncEnumerable<ChatAiStreamDelta> ChatStreamAsync(EngineChatRequest request, CancellationToken ct = default);
}