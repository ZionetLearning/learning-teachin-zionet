using Dapr.Client;
using Engine.Services.Clients.AccessorClient.Models;
using System.Net;

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
                $"threads/{threadId}/messages",
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

    public async Task<ChatMessage?> StoreMessageAsync(ChatMessage msg, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(StoreMessageAsync), nameof(AccessorClient));
        try
        {
            var created = await _daprClient.InvokeMethodAsync<ChatMessage, ChatMessage>(
                HttpMethod.Post,
                "accessor",
                "threads/message",
                msg,
                cancellationToken: ct
            );

            _logger.LogDebug("Message stored in thread {ThreadId}", msg.ThreadId);
            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store message for thread {ThreadId}", msg.ThreadId);
            throw;
        }
    }

    public async Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(string userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetThreadsForUserAsync), nameof(AccessorClient));
        try
        {
            var threads = await _daprClient.InvokeMethodAsync<List<ChatThread>>(
                HttpMethod.Get,
                "accessor",
                $"threads/{userId}",
                cancellationToken: ct
            );

            return threads ?? new List<ChatThread>();
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No threads found for user {UserId}", userId);
            return Array.Empty<ChatThread>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get threads for user {UserId}", userId);
            throw;
        }
    }
}