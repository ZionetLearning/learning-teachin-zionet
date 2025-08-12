using Dapr.Client;
using Engine.Models;
using System.Net;
using Engine.Constants;

namespace Engine.Services.Clients;

public class AccessorClient : IAccessorClient
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<AccessorClient> _logger;

    public AccessorClient(DaprClient daprClient, ILogger<AccessorClient> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    public async Task<ChatHistoryResponse?> GetChatHistoryAsync(string threadId)
    {
        try
        {
            _logger.LogInformation("Retrieving chat history for threadId: {ThreadId}", threadId);

            var response = await _daprClient.InvokeMethodAsync<ChatHistoryResponse?>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"/threads/{threadId}/messages");

            _logger.LogInformation("Successfully retrieved chat history for threadId: {ThreadId} with {MessageCount} messages",
                threadId, response?.Messages?.Count ?? 0);

            return response;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Chat history not found for threadId: {ThreadId} (404 from accessor)", threadId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve chat history for threadId: {ThreadId}", threadId);
            return null;
        }
    }

    public async Task<bool> StoreChatMessagesAsync(StoreChatMessagesRequest request)
    {
        try
        {
            _logger.LogInformation("Storing chat messages for threadId: {ThreadId}}",
                request.ThreadId);

            await _daprClient.InvokeMethodAsync(
                HttpMethod.Post,
                AppIds.Accessor,
                "/threads/message",
                request);

            _logger.LogInformation("Successfully stored chat messages for threadId: {ThreadId}", request.ThreadId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store chat messages for threadId: {ThreadId}", request.ThreadId);
            return false;
        }
    }
}