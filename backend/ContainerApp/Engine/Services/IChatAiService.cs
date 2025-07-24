using Engine.Models;

namespace Engine.Services;

public interface IChatAiService
{
    Task<AiResponseModel> ProcessAsync(AiRequestModel request, CancellationToken ct = default);
}