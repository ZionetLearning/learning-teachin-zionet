namespace Manager.Models;

// This Only for example!! not a real implementation
public class Event<TPayload>
{
    public string Type { get; set; } = string.Empty;
    public TPayload Payload { get; set; } = default!;
}

// This is an example payload for a chat message.to notify clients.

public class ChatMessagePayload
{
    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
