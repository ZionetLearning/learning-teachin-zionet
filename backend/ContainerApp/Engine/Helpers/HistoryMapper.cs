using System.Text.Json;
using Engine.Models.Chat;
using Engine.Services.Clients.AccessorClient.Models;

namespace Engine.Helpers;

public static class HistoryMapper
{
    public static ChatHistoryForFrontDto MapHistoryForFront(HistorySnapshotDto snapshot)
    {
        var messages = new List<ChatHistoryMessageDto>();

        if (snapshot.History.ValueKind == JsonValueKind.Object &&
            snapshot.History.TryGetProperty("messages", out var arr) &&
            arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var m in arr.EnumerateArray())
            {
                var role = GetRoleLabel(m);
                if (!IsUserOrAssistant(role))
                {
                    continue;
                }

                var text = GetFirstTextFromItems(m);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                var createdAt = GetCreatedAt(m);

                messages.Add(new ChatHistoryMessageDto
                {
                    Role = role!.ToLowerInvariant(),
                    Text = text!,
                    CreatedAt = createdAt
                });
            }
        }

        return new ChatHistoryForFrontDto
        {
            ChatId = snapshot.ThreadId,
            Name = snapshot.Name,
            ChatType = snapshot.ChatType,
            Messages = messages
        };
    }

    private static string? GetRoleLabel(JsonElement message)
    {
        if (!message.TryGetProperty("Role", out var roleEl))
        {
            return null;
        }

        if (roleEl.ValueKind == JsonValueKind.Object &&
            roleEl.TryGetProperty("Label", out var labelEl) &&
            labelEl.ValueKind == JsonValueKind.String)
        {
            return labelEl.GetString();
        }

        if (roleEl.ValueKind == JsonValueKind.String)
        {
            return roleEl.GetString();
        }

        return null;
    }

    private static bool IsUserOrAssistant(string? role)
        => role is not null &&
           (role.Equals("user", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("assistant", StringComparison.OrdinalIgnoreCase));

    private static string? GetFirstTextFromItems(JsonElement message)
    {
        if (!message.TryGetProperty("Items", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object &&
                item.TryGetProperty("Text", out var txt) &&
                txt.ValueKind == JsonValueKind.String)
            {
                var s = txt.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    return s;
                }
            }
        }

        return null;
    }

    private static DateTimeOffset? GetCreatedAt(JsonElement message)
    {
        if (message.TryGetProperty("Metadata", out var meta) &&
            meta.ValueKind == JsonValueKind.Object &&
            meta.TryGetProperty("CreatedAt", out var created) &&
            created.ValueKind == JsonValueKind.String &&
            DateTimeOffset.TryParse(created.GetString(), out var dto))
        {
            return dto;
        }

        return null;
    }
}
