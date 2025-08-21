using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Engine.Helpers
{
    public sealed class ChatMessageContentConverter : JsonConverter<ChatMessageContent>
    {
        public override ChatMessageContent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject for ChatMessageContent.");
            }

            string? role = null;
            string? content = null;
            string? modelId = null;
            JsonNode? metadataNode = null;
            JsonNode? usageNode = null;
            string? finishReason = null;
            long? createdAt = null;

            JsonArray? toolCalls = null;
            JsonArray? toolResults = null;
            JsonArray? itemsArr = null;

            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var obj = doc.RootElement;

                role = obj.GetPropertyOrNull("role")?.GetString();
                content = obj.GetPropertyOrNull("content")?.GetString();
                modelId = obj.GetPropertyOrNull("modelId")?.GetString();
                createdAt = obj.GetPropertyOrNull("createdAt")?.GetInt64();

                if (obj.TryGetProperty("metadata", out var mdProp))
                {
                    metadataNode = JsonNode.Parse(mdProp.GetRawText());
                }

                if (obj.TryGetProperty("usage", out var usageProp))
                {
                    usageNode = JsonNode.Parse(usageProp.GetRawText());
                }

                if (obj.TryGetProperty("finishReason", out var frProp))
                {
                    finishReason = frProp.GetString();
                }

                if (obj.TryGetProperty("toolCalls", out var tcProp) && tcProp.ValueKind == JsonValueKind.Array)
                {
                    toolCalls = JsonNode.Parse(tcProp.GetRawText()) as JsonArray;
                }

                if (obj.TryGetProperty("toolResults", out var trProp) && trProp.ValueKind == JsonValueKind.Array)
                {
                    toolResults = JsonNode.Parse(trProp.GetRawText()) as JsonArray;
                }

                if (obj.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    itemsArr = JsonNode.Parse(itemsProp.GetRawText()) as JsonArray;
                }
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                role = "user";
            }

            var author = role.ToLowerInvariant() switch
            {
                "system" => AuthorRole.System,
                "assistant" => AuthorRole.Assistant,
                "user" => AuthorRole.User,
                "developer" => AuthorRole.Developer,
                "tool" => AuthorRole.Tool,
                _ => AuthorRole.User
            };

            var meta = ToMutableCaseInsensitive(NodeToObj(metadataNode) as IReadOnlyDictionary<string, object?>)
                       ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            if (usageNode is not null)
            {
                meta["usage"] = NodeToObj(usageNode);
            }

            if (!string.IsNullOrWhiteSpace(finishReason))
            {
                meta["finish_reason"] = finishReason;
            }

            ChatMessageContent result;

            if (author == AuthorRole.Assistant && toolCalls is not null && toolCalls.Count > 0)
            {
                var col = new ChatMessageContentItemCollection();

                foreach (var n in toolCalls.OfType<JsonObject>())
                {
                    var fn = n["functionName"]?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(fn))
                    {
                        continue;
                    }

                    var plugin = n["pluginName"]?.GetValue<string>();
                    var id = n["id"]?.GetValue<string>()
                             ?? n["toolCallId"]?.GetValue<string>();

                    var args = NodeToKernelArguments(n.ContainsKey("arguments") ? n["arguments"] : null);

                    col.Add(new FunctionCallContent(fn!, plugin, id, args));
                }

                result = new ChatMessageContent(AuthorRole.Assistant, col, null, null, null, meta);
                if (!string.IsNullOrWhiteSpace(modelId))
                {
                    result.ModelId = modelId;
                }

                return result;
            }

            if (author == AuthorRole.Tool && toolResults is not null && toolResults.Count > 0)
            {
                var col = new ChatMessageContentItemCollection();

                foreach (var n in toolResults.OfType<JsonObject>())
                {
                    var fn = n["functionName"]?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(fn))
                    {
                        continue;
                    }

                    var plugin = n["pluginName"]?.GetValue<string>();
                    var callId = n["callId"]?.GetValue<string>()
                                 ?? n["toolCallId"]?.GetValue<string>()
                                 ?? n["id"]?.GetValue<string>();

                    var resultObj = NodeToObj(n["result"]);

                    col.Add(new FunctionResultContent(fn!, plugin, callId, resultObj));
                }

                result = new ChatMessageContent(AuthorRole.Tool, col, null, null, null, meta);
                if (!string.IsNullOrWhiteSpace(modelId))
                {
                    result.ModelId = modelId;
                }

                return result;
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                result = new ChatMessageContent(author, content, null, null, null, meta);
                if (!string.IsNullOrWhiteSpace(modelId))
                {
                    result.ModelId = modelId;
                }

                return result;
            }

            if (itemsArr is not null)
            {
                var col = ItemsArrayToCollection(itemsArr);
                if (col.Count > 0)
                {
                    result = new ChatMessageContent(author, col, null, null, null, meta);
                    if (!string.IsNullOrWhiteSpace(modelId))
                    {
                        result.ModelId = modelId;
                    }

                    return result;
                }
            }

            result = new ChatMessageContent(author, string.Empty, null, null, null, meta);
            if (!string.IsNullOrWhiteSpace(modelId))
            {
                result.ModelId = modelId;
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, ChatMessageContent value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("role", value.Role.ToString().ToLowerInvariant());

            writer.WriteNumber("createdAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            if (!string.IsNullOrWhiteSpace(value.ModelId))
            {
                writer.WriteString("modelId", value.ModelId);
            }

            var toolCalls = value.Items?.OfType<FunctionCallContent>().ToList() ?? new List<FunctionCallContent>();
            var toolResults = value.Items?.OfType<FunctionResultContent>().ToList() ?? new List<FunctionResultContent>();
            var texts = value.Items?
                .OfType<TextContent>()
                .Select(t => t.Text)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!)
                .ToList()
                ?? new List<string>();

            var content = string.Empty;

            if (texts.Count > 0)
            {
                content = string.Join("\n", texts);
            }
            else if (!string.IsNullOrWhiteSpace(value.Content))
            {
                content = value.Content!;
            }

            if (value.Role == AuthorRole.Assistant && toolCalls.Count > 0)
            {
                writer.WritePropertyName("toolCalls");
                writer.WriteStartArray();

                foreach (var c in toolCalls)
                {
                    writer.WriteStartObject();

                    writer.WriteString("functionName", c.FunctionName);

                    if (!string.IsNullOrWhiteSpace(c.PluginName))
                    {
                        writer.WriteString("pluginName", c.PluginName);
                    }

                    if (!string.IsNullOrWhiteSpace(c.Id))
                    {
                        writer.WriteString("id", c.Id);
                    }

                    if (c.Arguments is not null && c.Arguments.Count > 0)
                    {
                        writer.WritePropertyName("arguments");
                        WriteObjAsJson(writer, c.Arguments.ToDictionary(kv => kv.Key, kv => kv.Value), options);
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }
            else if (value.Role == AuthorRole.Tool && toolResults.Count > 0)
            {
                writer.WritePropertyName("toolResults");
                writer.WriteStartArray();

                foreach (var r in toolResults)
                {
                    writer.WriteStartObject();

                    writer.WriteString("functionName", r.FunctionName);

                    if (!string.IsNullOrWhiteSpace(r.PluginName))
                    {
                        writer.WriteString("pluginName", r.PluginName);
                    }

                    if (!string.IsNullOrWhiteSpace(r.CallId))
                    {
                        writer.WriteString("callId", r.CallId);
                    }

                    writer.WritePropertyName("result");
                    WriteObjAsJson(writer, r.Result, options);

                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    writer.WriteString("content", content);
                }

                if (value.Metadata is not null && value.Metadata.Count > 0)
                {
                    if (value.Metadata.TryGetValue("usage", out var usageVal) && usageVal is not null)
                    {
                        writer.WritePropertyName("usage");
                        WriteObjAsJson(writer, usageVal, options);
                    }

                    if (value.Metadata.TryGetValue("finish_reason", out var frVal) && frVal is not null)
                    {
                        writer.WriteString("finishReason", frVal.ToString());
                    }

                    var metaCopy = new Dictionary<string, object?>(value.Metadata, StringComparer.OrdinalIgnoreCase);

                    RemoveKeysIgnoreCase(metaCopy, "createdAt", "usage", "finish_reason");

                    if (metaCopy.Count > 0)
                    {
                        writer.WritePropertyName("metadata");
                        WriteObjAsJson(writer, metaCopy, options);
                    }
                }
            }

            writer.WriteEndObject();
        }

        private static KernelArguments? NodeToKernelArguments(JsonNode? node)
        {
            if (node is null)
            {
                return null;
            }

            try
            {
                if (node is JsonValue v && v.TryGetValue<string>(out var s) && !string.IsNullOrWhiteSpace(s))
                {
                    var parsed = JsonNode.Parse(s) as JsonObject;
                    if (parsed is null)
                    {
                        return null;
                    }

                    var ka = new KernelArguments();
                    foreach (var kv in parsed)
                    {
                        ka[kv.Key] = NodeToObj(kv.Value);
                    }

                    return ka;
                }

                if (node is JsonObject obj)
                {
                    var ka = new KernelArguments();
                    foreach (var kv in obj)
                    {
                        ka[kv.Key] = NodeToObj(kv.Value);
                    }

                    return ka;
                }
            }
            catch
            {
            }

            return null;
        }

        private static void WriteObjAsJson(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            try
            {
                var node = JsonSerializer.SerializeToNode(value, value.GetType(), options);
                if (node is null)
                {
                    writer.WriteNullValue();
                    return;
                }

                node.WriteTo(writer);
            }
            catch
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private static ChatMessageContentItemCollection ItemsArrayToCollection(JsonArray items)
        {
            var col = new ChatMessageContentItemCollection();

            foreach (var it in items.OfType<JsonObject>())
            {
                var type = it["type"]?.GetValue<string>()?.ToLowerInvariant();

                if (type == "text")
                {
                    var text = it["text"]?.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        col.Add(new TextContent(text));
                    }
                }
            }

            return col;
        }

        private static object? NodeToObj(JsonNode? node)
        {
            if (node is null)
            {
                return null;
            }

            if (node is JsonValue v)
            {
                if (v.TryGetValue<string>(out var s))
                {
                    return s;
                }

                if (v.TryGetValue<long>(out var l))
                {
                    return l;
                }

                if (v.TryGetValue<double>(out var d))
                {
                    return d;
                }

                if (v.TryGetValue<bool>(out var b))
                {
                    return b;
                }
            }

            if (node is JsonArray arr)
            {
                var list = new List<object?>();
                foreach (var n in arr)
                {
                    list.Add(NodeToObj(n));
                }

                return list;
            }

            if (node is JsonObject obj)
            {
                var dict = new Dictionary<string, object?>();
                foreach (var kv in obj)
                {
                    dict[kv.Key] = NodeToObj(kv.Value);
                }

                return dict;
            }

            return node.ToJsonString();
        }

        private static Dictionary<string, object?>? ToMutableCaseInsensitive(IReadOnlyDictionary<string, object?>? src)
        {
            if (src is null)
            {
                return null;
            }

            var d = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in src)
            {
                d[kv.Key] = kv.Value;
            }

            return d;
        }

        private static void RemoveKeysIgnoreCase(IDictionary<string, object?> dict, params string[] keys)
        {
            if (dict.Count == 0)
            {
                return;
            }

            var targets = new HashSet<string>(keys.Select(k => k.ToLowerInvariant()));
            var toRemove = new List<string>();

            foreach (var k in dict.Keys)
            {
                if (targets.Contains(k.ToLowerInvariant()))
                {
                    toRemove.Add(k);
                }
            }

            foreach (var k in toRemove)
            {
                dict.Remove(k);
            }
        }
    }
}

file static class JsonElementExt
{
    public static JsonElement? GetPropertyOrNull(this JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var v))
        {
            return v;
        }

        return null;
    }
}
