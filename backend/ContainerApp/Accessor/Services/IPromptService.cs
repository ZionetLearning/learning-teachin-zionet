using Accessor.Models.Prompts;

namespace Accessor.Services;

public interface IPromptService
{
    Task<PromptResponse> CreatePromptAsync(CreatePromptRequest request, CancellationToken cancellationToken = default);
    Task<PromptResponse?> GetLatestPromptAsync(string promptKey, CancellationToken cancellationToken = default);
    Task<List<PromptResponse>> GetAllVersionsAsync(string promptKey, CancellationToken cancellationToken = default);
    Task<List<PromptResponse>> GetLatestPromptsAsync(IEnumerable<string> promptKeys, CancellationToken cancellationToken = default); // <-- Added
    Task InitializeDefaultPromptsAsync();
}
