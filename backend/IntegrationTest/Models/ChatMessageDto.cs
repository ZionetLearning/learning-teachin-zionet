// IntegrationTests.Models/ChatMessageDto.cs
namespace IntegrationTests.Models;

public sealed class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public string UserId { get; set; } = "";
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTimeOffset Timestamp { get; set; }
}
