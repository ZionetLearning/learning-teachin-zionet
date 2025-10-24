using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models.Notification;
using Manager.Models.Chat;
using Manager.Models.Users;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.AI;

public abstract class AiChatTestBase(
    HttpClientFixture fixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(fixture, outputHelper, signalRFixture)
{
    public override async Task InitializeAsync()
    {
        await ClientFixture.LoginAsync(Role.Admin);

        await EnsureSignalRStartedAsync();

        SignalRFixture.ClearReceivedMessages();
    }

    protected async Task<(string RequestId, ReceivedEvent Event, AIChatStreamResponse[] Frames)>
        PostChatAndWaitAsync(ChatRequest request, TimeSpan? timeout = null)
    {
        var resp = await Client.PostAsJsonAsync(AiRoutes.PostNewMessage, request);
        resp.EnsureSuccessStatusCode();

        var doc = await resp.Content.ReadFromJsonAsync<JsonElement>();
        doc.TryGetProperty("requestId", out var ridEl).Should().BeTrue();
        var requestId = ridEl.GetString();
        requestId.Should().NotBeNullOrWhiteSpace();
    
        var (ev, frames) = await WaitForChatResponseAsync(requestId!, timeout);

        return (requestId!, ev, frames);
    }

    protected async Task<(ReceivedEvent Event, AIChatStreamResponse[] Frames)>
        WaitForChatResponseAsync(string requestId, TimeSpan? timeout = null)
    {
        var frames = await SignalRFixture.WaitForStreamUntilFinalAsync(requestId, timeout);

        frames.Should().NotBeNull();
        frames.Count.Should().BeGreaterThan(0, "expected streaming frames for the chat response");

        var mapped = frames.Select(f => new AIChatStreamResponse
        {
            RequestId = requestId,
            ThreadId = Guid.TryParse(TryGetString(f.Event.Payload, "threadId"), out var tid) ? tid : Guid.Empty,
            UserId = Guid.TryParse(TryGetString(f.Event.Payload, "userId"), out var uid) ? uid : Guid.Empty,
            ChatName = TryGetString(f.Event.Payload, "chatName") ?? string.Empty,
            Delta = TryGetString(f.Event.Payload, "delta") ?? TryGetString(f.Event.Payload, "text"),
            Sequence = f.Event.SequenceNumber,
            Stage = Enum.TryParse<ChatStreamStage>(TryGetString(f.Event.Payload, "stage"), true, out var s) ? s : (TryGetEnum<ChatStreamStage>(f.Event.Payload, "stage", out var s2) ? s2 : ChatStreamStage.Unknown),
            IsFinal = TryGetBool(f.Event.Payload, "isFinal", out var fin) && fin,
            ElapsedMs = long.TryParse(TryGetString(f.Event.Payload, "elapsedMs"), out var ms) ? ms : 0,
            ToolCall = TryGetString(f.Event.Payload, "toolCall"),
            ToolResult = TryGetString(f.Event.Payload, "toolResult")
        }).ToArray();

        var last = frames.Last();
        var payload = JsonSerializer.SerializeToElement(new
        {
            requestId,
            chatName = mapped.LastOrDefault()?.ChatName ?? string.Empty,
            sequence = mapped.LastOrDefault()?.Sequence ?? 0,
            stage = mapped.LastOrDefault()?.Stage.ToString(),
            isFinal = mapped.LastOrDefault()?.IsFinal ?? false
        });

        var ev = new ReceivedEvent
        {
            Event = new UserEvent<JsonElement>
            {
                EventType = EventType.ChatAiAnswer,
                Payload = payload
            },
            ReceivedAt = last.ReceivedAt
        };

        return (ev, mapped);
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
            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt64(out var i)) { value = i != 0; return true; }
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