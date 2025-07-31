using System.Text.Json;

namespace NotificationManager.Models;

public record Argument
{
    public required string Sender { get; set; }
    public required JsonElement Text { get; set; }
}
