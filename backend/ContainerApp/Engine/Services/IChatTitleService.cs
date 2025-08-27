using Microsoft.SemanticKernel.ChatCompletion;

namespace Engine.Services;

public interface IChatTitleService
{
    Task<string> GenerateTitleAsync(ChatHistory history, CancellationToken ct = default);
}