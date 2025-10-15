using Engine.Models.Chat;

namespace Engine.Services;

public interface IChatAiService
{
    Task<ChatAiServiceResponse> ChatHandlerAsync(ChatAiServiceRequest request, CancellationToken ct = default);

    IAsyncEnumerable<ChatAiStreamDelta> ChatStreamAsync(ChatAiServiceRequest request, CancellationToken ct = default);
}