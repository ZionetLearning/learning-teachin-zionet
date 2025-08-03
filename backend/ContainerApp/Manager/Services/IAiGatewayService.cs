using Manager.Models;

namespace Manager.Services;

public interface IAiGatewayService
{
    Task<string> SendQuestionAsync(string question, CancellationToken ct = default);
    Task SaveAnswerAsync(AiResponseModel response, CancellationToken ct = default);
    Task<string?> GetAnswerAsync(string id, CancellationToken ct = default);
}