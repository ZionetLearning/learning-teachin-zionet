using Engine.Models.Chat;

namespace Engine.Services;

public interface IChatAiService
{
    Task<AiServiceResponse> ChatHandlerAsync(ChatAiServiseRequest request, CancellationToken ct = default);
}