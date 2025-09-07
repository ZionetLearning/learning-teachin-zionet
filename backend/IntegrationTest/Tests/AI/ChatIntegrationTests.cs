using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using Manager.Models.Chat;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.AI;

[Collection("Shared test collection")]
public class ChatIntegrationTests(
    SharedTestFixture sharedFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : AiChatTestBase(sharedFixture.HttpFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    private readonly SharedTestFixture _shared = sharedFixture;

    public override async Task InitializeAsync()
    {

    }

    [Fact(DisplayName = "Chat AI integration test")]
    public async Task Post_new_chat()
    {
        var user = _shared.UserFixture.TestUser;

        SignalRFixture.UseUserId(user.UserId.ToString());

        await _shared.GetAuthenticatedTokenAsync();

        await _shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);

        var chatId1 = Guid.NewGuid();

        var chatRequest1 = new ChatRequest
        {
            ThreadId = chatId1.ToString(),
            UserId = user.UserId.ToString(),
            UserMessage = "Remember number 42",
            ChatType = ChatType.Default

        };

        var r1 = await Client.PostAsJsonAsync(AiRoutes.PostNewMessage, chatRequest1);
        r1.ShouldBeOk();

        var doc = await r1.Content.ReadFromJsonAsync<JsonElement>();
        doc.TryGetProperty("requestId", out var ridEl).Should().BeTrue();
        var requestId1 = ridEl.GetString();
        requestId1.Should().NotBeNullOrWhiteSpace();

        var received1 = await SignalRFixture.WaitForChatAiAnswerAsync(requestId1, TimeSpan.FromSeconds(30));
        received1.Should().NotBeNull();

        var chatHistoryAfterRequest1 = await AIChatHelper.CheckCountMessageInChatHistory(Client, chatId1, user.UserId, waitMessages:2, timeoutSeconds: 30);

        chatHistoryAfterRequest1.Messages.Count.Should().Be(2);

        var chatRequest2 = new ChatRequest
        {
            ThreadId = chatId1.ToString(),
            UserId = user.UserId.ToString(),
            UserMessage = "What number did you remember?",
            ChatType = ChatType.Default

        };

        var r2 = await Client.PostAsJsonAsync(AiRoutes.PostNewMessage, chatRequest2);
        r2.ShouldBeOk();

        var doc2 = await r2.Content.ReadFromJsonAsync<JsonElement>();
        doc2.TryGetProperty("requestId", out var ridEl2).Should().BeTrue();
        var requestId2 = ridEl2.GetString();
        requestId2.Should().NotBeNullOrWhiteSpace();

        var received2 = await SignalRFixture.WaitForChatAiAnswerAsync(requestId2, TimeSpan.FromSeconds(30));
        received2.Should().NotBeNull();
        var msg2 = received2!.Event.Payload.GetProperty("assistantMessage").GetString() ?? "";

        var regexCheck = new Regex(@"\b42\b|\bforty[-\s]?two\b", RegexOptions.IgnoreCase);
        msg2.Should().MatchRegex(regexCheck);

        var chatHistoryAfterRequest2 = await AIChatHelper.CheckCountMessageInChatHistory(Client, chatId1, user.UserId, waitMessages: 4, timeoutSeconds: 30);

        chatHistoryAfterRequest2.Messages.Count.Should().Be(4);

        var text = chatHistoryAfterRequest2.Messages[^1].Text ?? string.Empty;
        text.Should().MatchRegex(regexCheck);

    }
}