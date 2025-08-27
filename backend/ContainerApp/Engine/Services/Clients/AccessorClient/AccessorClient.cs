using System.Net;
using System.Text.Json;
using Dapr.Client;
using Engine.Services.Clients.AccessorClient.Models;

namespace Engine.Services.Clients.AccessorClient;

public class AccessorClient(ILogger<AccessorClient> logger, DaprClient daprClient) : IAccessorClient
{
    private readonly ILogger<AccessorClient> _logger = logger;
    private readonly DaprClient _daprClient = daprClient;

    public async Task<IReadOnlyList<ChatMessage>> GetChatHistoryAsync(Guid threadId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetChatHistoryAsync), nameof(AccessorClient));
        try
        {
            var messages = await _daprClient.InvokeMethodAsync<List<ChatMessage>>(
                HttpMethod.Get,
                "accessor",
                $"chats/{threadId}/messages",
                cancellationToken: ct
            );

            return messages ?? new List<ChatMessage>(); //think what return? 
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Thread {ThreadId} not found, returning empty history", threadId);
            return Array.Empty<ChatMessage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get chat history for thread {ThreadId}", threadId);
            throw;
        }
    }

    public async Task<HistorySnapshotDto> GetHistorySnapshotAsync(Guid threadId, Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("GET history snapshot for thread {ThreadId}", threadId);

        try
        {
            var dto = await _daprClient.InvokeMethodAsync<HistorySnapshotDto>(
                HttpMethod.Get,
                "accessor",
                $"chats/{threadId}/{userId}/history",
                cancellationToken: ct);

            return dto;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("History snapshot for thread {ThreadId} not found, returning empty", threadId);

            using var doc = JsonDocument.Parse("""{"messages":[]}""");
            return new HistorySnapshotDto
            {
                ThreadId = threadId,
                UserId = userId,
                Name = "defaultName",
                ChatType = "default",
                History = doc.RootElement.Clone()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get history snapshot for chatId: {ChatId}", threadId);
            throw;
        }
    }

    public async Task<HistorySnapshotDto> UpsertHistorySnapshotAsync(UpsertHistoryRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("UPSERT history snapshot for chatId: {ChatId}", request.ThreadId);

        try
        {
            var dto = await _daprClient.InvokeMethodAsync<UpsertHistoryRequest, HistorySnapshotDto>(
                HttpMethod.Post,
                "accessor",
                "chats/history",
                request,
                cancellationToken: ct);

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert history snapshot for chatId {ChatId}", request.ThreadId);
            throw;
        }
    }
}