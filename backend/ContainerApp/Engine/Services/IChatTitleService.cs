using Microsoft.SemanticKernel.ChatCompletion;

namespace Engine.Services;

public interface IChatTitleService
{
    Task<string> GenerateAsync(ChatHistory history, CancellationToken ct = default);
}