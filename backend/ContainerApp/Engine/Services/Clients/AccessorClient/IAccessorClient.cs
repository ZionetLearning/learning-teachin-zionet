using Engine.Models.Prompts;
using Engine.Models.Users;
using Engine.Options;
using Engine.Services.Clients.AccessorClient.Models;

namespace Engine.Services.Clients.AccessorClient;

public interface IAccessorClient
{
    Task<IReadOnlyList<ChatMessage>> GetChatHistoryAsync(Guid threadId, CancellationToken ct = default);
    Task<HistorySnapshotDto> GetHistorySnapshotAsync(Guid threadId, Guid userId, CancellationToken ct = default);
    Task<HistorySnapshotDto> UpsertHistorySnapshotAsync(UpsertHistoryRequest request, CancellationToken ct = default);

    Task<AttemptDetailsResponse> GetAttemptDetailsAsync(Guid userId, Guid attemptId, CancellationToken ct = default);

    // Prompts
    Task<PromptResponse> CreatePromptAsync(CreatePromptRequest request, CancellationToken ct = default);
    Task<PromptResponse?> GetPromptAsync(PromptConfiguration config, CancellationToken ct = default);
    Task<GetPromptsBatchResponse> GetPromptsBatchAsync(IEnumerable<PromptConfiguration> configs, CancellationToken ct = default);

    // User Interests
    Task<List<string>> GetUserInterestsAsync(Guid userId, CancellationToken ct);
    Task<UserData?> GetUserAsync(Guid userId);

}

