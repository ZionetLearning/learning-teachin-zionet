using System.Text.Json;
using Engine.Models.Chat;
using Engine.Services.Clients.AccessorClient.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

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

    #region JsonParser
    public static JsonElement ToJsonElementCompat<T>(T value)
    {
        return System.Text.Json.JsonSerializer.SerializeToElement(value!);

    }

    public static ChatHistory CloneToChatHistory(ChatHistory source)
    {
        var h = new ChatHistory();
        foreach (var m in source)
        {
            h.Add(DeepCloneMessage(m));
        }

        return h;
    }

    public static ChatMessageContent DeepCloneMessage(ChatMessageContent msg)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(msg);
        var clone = System.Text.Json.JsonSerializer.Deserialize<ChatMessageContent>(json);

        return clone!;
    }

    public static void AppendDelta(ChatHistory baseHistory, ChatHistory srcWithNew)
    {
        if (baseHistory.Count >= srcWithNew.Count)
        {
            return;
        }

        for (var i = baseHistory.Count; i < srcWithNew.Count; i++)
        {
            baseHistory.Add(DeepCloneMessage(srcWithNew[i]));
        }
    }

    public static ChatHistory ToChatHistoryFromElement(JsonElement element)
    {
        var history = new ChatHistory();

        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return history;
        }

        if (!element.TryGetProperty("messages", out var messages) || messages.ValueKind != JsonValueKind.Array)
        {
            return history;
        }

        foreach (var msgEl in messages.EnumerateArray())
        {
            var role = AuthorRole.User;
            if (msgEl.TryGetProperty("Role", out var roleObj) && roleObj.ValueKind == JsonValueKind.Object)
            {
                var label = roleObj.GetPropertyOrNull("Label")?.GetString();
                role = MapRole(label);
            }
            else
            {
                var label = msgEl.GetPropertyOrNull("role")?.GetString();
                if (!string.IsNullOrWhiteSpace(label))
                {
                    role = MapRole(label);
                }
            }

            var itemsCol = new ChatMessageContentItemCollection();

            if (msgEl.TryGetProperty("Items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var it in itemsEl.EnumerateArray())
                {
                    var typeStr = it.GetPropertyOrNull("$type")?.GetString();

                    switch (typeStr)
                    {
                        case "TextContent":
                        {
                            var text = it.GetPropertyOrNull("Text")?.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                itemsCol.Add(new TextContent(text));
                            }

                            break;
                        }

                        case "FunctionCallContent":
                        {
                            var fn = it.GetPropertyOrNull("FunctionName")?.GetString();
                            if (string.IsNullOrWhiteSpace(fn))
                            {
                                break;
                            }

                            var plugin = it.GetPropertyOrNull("PluginName")?.GetString();
                            var id = it.GetPropertyOrNull("Id")?.GetString();
                            var argsKA = ElementToKernelArguments(it.GetPropertyOrNull("Arguments"));

                            itemsCol.Add(new FunctionCallContent(fn!, plugin, id, argsKA));
                            break;
                        }

                        case "FunctionResultContent":
                        {
                            var fn = it.GetPropertyOrNull("FunctionName")?.GetString() ?? string.Empty;
                            var plugin = it.GetPropertyOrNull("PluginName")?.GetString();
                            var callId = it.GetPropertyOrNull("CallId")?.GetString();
                            object? resultObj = null;
                            var resEl = it.GetPropertyOrNull("Result");
                            if (resEl.HasValue)
                            {
                                resultObj = ElementToObj(resEl.Value);
                            }

                            itemsCol.Add(new FunctionResultContent(fn, plugin, callId, resultObj));
                            break;
                        }

                        default:
                        {
                            var text = it.GetPropertyOrNull("Text")?.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                itemsCol.Add(new TextContent(text));
                            }

                            break;
                        }
                    }
                }
            }

            var content = msgEl.GetPropertyOrNull("content")?.GetString() ?? string.Empty;

            var modelId = msgEl.GetPropertyOrNull("ModelId")?.GetString()
                              ?? msgEl.GetPropertyOrNull("modelId")?.GetString();

            var metadata = ReadMetadataFromMessage(msgEl);

            ChatMessageContent message;
            if (itemsCol.Count > 0)
            {
                message = new ChatMessageContent(role, itemsCol, null, null, null, metadata);
            }
            else
            {
                message = new ChatMessageContent(role, content, null, null, null, metadata);
            }

            if (!string.IsNullOrWhiteSpace(modelId))
            {
                message.ModelId = modelId;
            }

            history.Add(message);
        }

        return history;
    }

    public static AuthorRole MapRole(string? label)
    {
        switch (label?.Trim().ToLowerInvariant())
        {
            case "system":
                return AuthorRole.System;
            case "assistant":
                return AuthorRole.Assistant;
            case "developer":
                return AuthorRole.Developer;
            case "tool":
                return AuthorRole.Tool;
            case "user":
            default:
                return AuthorRole.User;
        }
    }

    public static KernelArguments? ElementToKernelArguments(JsonElement? elNullable)
    {
        if (!elNullable.HasValue)
        {
            return null;
        }

        var el = elNullable.Value;
        if (el.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var ka = new KernelArguments();
        foreach (var p in el.EnumerateObject())
        {
            ka[p.Name] = ElementToObj(p.Value);
        }

        return ka;
    }

    private static object? ElementToObj(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.String:
                return el.GetString();

            case JsonValueKind.Number:
            {
                if (el.TryGetInt64(out var l))
                {
                    return l;
                }

                if (el.TryGetDouble(out var d))
                {
                    return d;
                }

                return el.GetRawText();
            }

            case JsonValueKind.True:
            case JsonValueKind.False:
                return el.GetBoolean();

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;

            case JsonValueKind.Array:
            {
                var list = new List<object?>();
                foreach (var item in el.EnumerateArray())
                {
                    list.Add(ElementToObj(item));
                }

                return list;
            }

            case JsonValueKind.Object:
            {
                var dict = new Dictionary<string, object?>();
                foreach (var p in el.EnumerateObject())
                {
                    dict[p.Name] = ElementToObj(p.Value);
                }

                return dict;
            }

            default:
                return el.GetRawText();
        }
    }

    private static IReadOnlyDictionary<string, object?>? ReadMetadataFromMessage(JsonElement msgEl)
    {
        if (msgEl.TryGetProperty("Metadata", out var metaEl) && metaEl.ValueKind == JsonValueKind.Object)
        {
            return (IReadOnlyDictionary<string, object?>)ElementToObj(metaEl)!;
        }

        if (msgEl.TryGetProperty("metadata", out metaEl) && metaEl.ValueKind == JsonValueKind.Object)
        {
            return (IReadOnlyDictionary<string, object?>)ElementToObj(metaEl)!;
        }

        return null;
    }
    #endregion
    public static void AddUserMessage(
           this ChatHistory history,
           string text,
           DateTimeOffset createdAt)
    {
        var items = new ChatMessageContentItemCollection
        {
            new TextContent(text)
        };

        var metadata = new Dictionary<string, object?>
        {
            ["CreatedAt"] = createdAt.ToString("o")
        };

        var msg = new ChatMessageContent(
            AuthorRole.User,
            items,
            null,
            null,
            null,
            metadata);

        history.Add(msg);
    }

    public static void AddUserMessageNow(this ChatHistory history, string text) =>
        history.AddUserMessage(text, DateTimeOffset.UtcNow);

    public static JsonElement SerializeHistory(ChatHistory history)
    {
        var envelope = new HistoryEnvelope { Messages = history.ToList() };
        return ToJsonElementCompat(envelope);
    }
}

file static class JsonElementExt
{
    public static JsonElement? GetPropertyOrNull(this JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var v))
        {
            return v;
        }

        return null;
    }
}
