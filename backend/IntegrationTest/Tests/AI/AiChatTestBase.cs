using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models.Notification;
using Manager.Models.Chat;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.AI;

public abstract class AiChatTestBase(
    HttpTestFixture fixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(fixture, outputHelper, signalRFixture)
{
    protected async Task<(string RequestId, ReceivedEvent Event, string AssistantMessage, string ChatName, IReadOnlyList<string> ToolCalls, IReadOnlyList<string> ToolResults)>
        PostChatAndWaitAsync(ChatRequest request, TimeSpan? timeout = null)
    {
        var resp = await Client.PostAsJsonAsync(AiRoutes.PostNewMessage, request);
        resp.EnsureSuccessStatusCode();

        var doc = await resp.Content.ReadFromJsonAsync<JsonElement>();
        doc.TryGetProperty("requestId", out var ridEl).Should().BeTrue();
        var requestId = ridEl.GetString();
        requestId.Should().NotBeNullOrWhiteSpace();

        var (ev, finalText, chatName, toolCalls, toolResults) = await WaitForAiChatFinalAsync(requestId!, timeout);

        return (requestId!, ev, finalText, chatName, toolCalls, toolResults);
    }

    public async Task<ReceivedEvent?> WaitForChatAiAnswerAsync(string requestId, TimeSpan? timeout = null) =>
        await WaitForEventAsync(
            e => e.EventType == EventType.ChatAiAnswer &&
                 e.Payload.ValueKind == JsonValueKind.Object &&
                 e.Payload.TryGetProperty("requestId", out var rid) &&
                 rid.GetString() == requestId,
            timeout);


    protected async Task<(ReceivedEvent Event, string AssistantMessage, string ChatName, IReadOnlyList<string> ToolCalls, IReadOnlyList<string> ToolResults)>
        WaitForAiChatFinalAsync(string requestId, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(30);
        var deadline = DateTime.UtcNow + timeout.Value;

        var sb = new System.Text.StringBuilder();
        string chatName = string.Empty;
        var toolCalls = new List<string>();
        var toolResults = new List<string>();
        ReceivedEvent? lastEvent = null;
        var seenSequences = new HashSet<int>();

        while (DateTime.UtcNow < deadline)
        {
            var snapshot = SignalRFixture.GetReceivedEvents();

            var streamEvents = snapshot
                .Where(e => e.Event.EventType == EventType.ChatAiAnswer
                            && e.Event.Payload.ValueKind == JsonValueKind.Object
                            && TryGetString(e.Event.Payload, "requestId") == requestId)
                .ToList();

            foreach (var ev in streamEvents)
            {
                var p = ev.Event.Payload;

                var maybeName = TryGetString(p, "chatName");
                if (!string.IsNullOrEmpty(maybeName)) chatName = maybeName!;

                if (p.TryGetProperty("sequence", out var seqEl) && seqEl.ValueKind == JsonValueKind.Number)
                {
                    var seq = seqEl.GetInt32();
                    if (!seenSequences.Add(seq)) continue;
                }

                if (!TryGetEnum(p, "stage", out ChatStreamStage stage))
                {
                    var stageStr = TryGetString(p, "stage") ?? string.Empty;
                    Enum.TryParse(stageStr, ignoreCase: true, out stage);
                }

                switch (stage)
                {
                    case ChatStreamStage.Model:
                        {
                            var delta = TryGetString(p, "delta");
                            if (!string.IsNullOrEmpty(delta)) sb.Append(delta);
                            break;
                        }

                    case ChatStreamStage.Tool:
                        {
                            var name = TryGetString(p, "toolCall") ?? TryGetString(p, "tool");
                            if (!string.IsNullOrEmpty(name))
                                toolCalls.Add(name);
                            break;
                        }

                    case ChatStreamStage.ToolResult:
                        {
                            var result = TryGetString(p, "toolResult");
                            if (!string.IsNullOrEmpty(result))
                                toolResults.Add(result);
                            break;
                        }

                    case ChatStreamStage.Final:
                        {
                            lastEvent = ev;

                            var isFinal = TryGetBool(p, "isFinal", out var f) && f;
                            if (isFinal)
                                return (ev, sb.ToString(), chatName, toolCalls, toolResults);

                            break;
                        }

                    case ChatStreamStage.Canceled:
                    case ChatStreamStage.Expired:
                    default:
                        break;
                }

                lastEvent = ev;
            }

            await Task.Delay(50);
        }

        lastEvent.Should().NotBeNull("stream final was not received within timeout");
        return (lastEvent!, sb.ToString(), chatName, toolCalls, toolResults);
    }

    private static string? TryGetString(JsonElement obj, string prop)
        => obj.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() : null;

    private static bool TryGetBool(JsonElement obj, string prop, out bool value)
    {
        value = false;
        if (obj.TryGetProperty(prop, out var el))
        {
            if (el.ValueKind == JsonValueKind.True) { value = true; return true; }
            if (el.ValueKind == JsonValueKind.False) { value = false; return true; }
        }
        return false;
    }

    private static bool TryGetEnum<TEnum>(JsonElement obj, string prop, out TEnum value)
        where TEnum : struct, Enum
    {
        value = default;
        if (!obj.TryGetProperty(prop, out var el)) return false;

        if (el.ValueKind == JsonValueKind.String)
        {
            var s = el.GetString();
            return Enum.TryParse(s, ignoreCase: true, out value);
        }
        if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var i))
        {
            value = (TEnum)Enum.ToObject(typeof(TEnum), i);
            return true;
        }
        return false;
    }
}