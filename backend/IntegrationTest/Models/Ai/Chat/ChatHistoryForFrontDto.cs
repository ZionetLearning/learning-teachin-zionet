using Manager.Services.Clients.Engine.Models;
using System.Text.Json.Serialization;

namespace IntegrationTests.Models.Ai.Chat;


public class ChatHistoryForFrontDto
{
    [JsonPropertyName("chatId")]
    public Guid ChatId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("chatType")]
    public string ChatType { get; set; } = string.Empty;
    [JsonPropertyName("chatType")]
    public IReadOnlyList<ChatHistoryMessageDto> Messages { get; set; } = new List<ChatHistoryMessageDto>();
    
}