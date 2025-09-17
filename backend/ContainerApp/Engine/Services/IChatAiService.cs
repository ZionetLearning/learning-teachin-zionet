using Engine.Models.Chat;

namespace Engine.Services;

public interface IChatAiService
{
    Task<ChatAiServiceResponse> ChatHandlerAsync(ChatAiServiseRequest request, CancellationToken ct = default);

    IAsyncEnumerable<ChatAiStreamDelta> ChatStreamAsync(ChatAiServiseRequest request, CancellationToken ct = default);
}