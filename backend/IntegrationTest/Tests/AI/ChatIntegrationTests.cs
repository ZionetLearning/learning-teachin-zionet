using FluentAssertions;
using IntegrationTests.Fixtures;
using Manager.Models.Chat;
using Manager.Models.Users;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.AI;

[Collection("IntegrationTests")]
public class ChatIntegrationTests(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : AiChatTestBase(httpClientFixture, outputHelper, signalRFixture)
{
    [Fact(DisplayName = "Chat AI integration test")]
    public async Task Post_new_chat()
    {
        // Get the logged-in user's information
        var userInfo = ClientFixture.GetUserInfo(Role.Admin);

        var chatId1 = Guid.NewGuid();

        var (req1, ev1, frames1) = await PostChatAndWaitAsync(new ChatRequest
        {
            ThreadId = chatId1.ToString(),
            UserMessage = "Remember number 42",
            ChatType = ChatType.Default
        }, TimeSpan.FromSeconds(60));

        var chatName1 = frames1.Last().ChatName;
        chatName1.Should().NotBeNullOrWhiteSpace();
        chatName1.Should().NotBe("New chat");

        var fallbackTitlePattern = new Regex(@"^\d{4}_\d{2}_\d{2}$");
        fallbackTitlePattern.IsMatch(chatName1).Should().BeFalse($"_chatTitleService.GenerateTitleAsync not work properly. Name was: {chatName1}");

        var history1 = await AIChatHelper.CheckCountMessageInChatHistory(Client, chatId1, userInfo.UserId, 2, 30);
        history1.Messages.Count.Should().Be(2);

        var chatHistoryAfterRequest1 = await AIChatHelper.CheckCountMessageInChatHistory(Client, chatId1, userInfo.UserId, waitMessages: 2, timeoutSeconds: 30);
        chatHistoryAfterRequest1.Messages.Count.Should().Be(2);

        var (req2, ev2, frames2) = await PostChatAndWaitAsync(new ChatRequest
        {
            ThreadId = chatId1.ToString(),
            UserMessage = "What number did you remember?",
            ChatType = ChatType.Default
        }, TimeSpan.FromSeconds(60));

        var chatName2 = frames2.Last().ChatName;
        chatName2.Should().Be(chatName1);

        var regexCheck = new Regex(@"\b42\b|\bforty[-\s]?two\b", RegexOptions.IgnoreCase);
        string combined2 = string.Concat(frames2.Where(f => f.Stage == ChatStreamStage.Model).Select(f => f.Delta)) ?? string.Empty;
        combined2.Should().MatchRegex(regexCheck);

        var chatHistoryAfterRequest2 = await AIChatHelper.CheckCountMessageInChatHistory(Client, chatId1, userInfo.UserId, waitMessages: 4, timeoutSeconds: 30);
        chatHistoryAfterRequest2.Messages.Count.Should().Be(4);

        var text = chatHistoryAfterRequest2.Messages[^1].Text ?? string.Empty;
        text.Should().MatchRegex(regexCheck);

        var (req3, ev3, frames3) = await PostChatAndWaitAsync(new ChatRequest
        {
            ThreadId = chatId1.ToString(),
            UserMessage = "Give me the current time in ISO-8601 (UTC). Return only the timestamp.",
            ChatType = ChatType.Default
        }, TimeSpan.FromSeconds(60));

        var chatName3 = frames3.Last().ChatName;
        chatName3.Should().Be(chatName1);

        var combined3 = string.Concat(frames3.Where(f => f.Stage == ChatStreamStage.Model).Select(f => f.Delta)) ?? string.Empty;

        DateTimeOffset parsed;
        DateTimeOffset.TryParseExact(
            combined3.Trim(),
            "O",
            formatProvider: null,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out parsed
        ).Should().BeTrue("assistantMessage about time must be ISO-8601 (O format)");

        var delta = (DateTimeOffset.UtcNow - parsed).Duration();
        delta.Should().BeLessThan(TimeSpan.FromSeconds(300), "time should come from TimePlugin/clock");

        frames3.Any(f => f.Stage == ChatStreamStage.Tool && (f.ToolCall ?? string.Empty).Equals("Time-current_time", StringComparison.OrdinalIgnoreCase))
               .Should().BeTrue("time tool should be invoked and present in SignalR stream");
    }

    [Fact(DisplayName = "Global chat: pageContext is stored and used by LLM")]
    public async Task GlobalChat_Uses_PageContext()
    {
        var userInfo = ClientFixture.GetUserInfo(Role.Admin);

        var chatId = Guid.NewGuid();

        var pageContextPayload = new
        {
            pageId = "lesson-123",
            topic = "fractions",
            magicNumber = 777
        };

        var pageContextJson = JsonSerializer.Serialize(pageContextPayload);

        var (req, ev, frames) = await PostGlobalChatAndWaitAsync(
            new ChatRequest
            {
                ThreadId = chatId.ToString(),
                UserMessage = "What is the magic number from the page context? Answer only the number.",
                ChatType = ChatType.Global,
                PageContext = new()
                {
                    JsonContext = pageContextJson
                }
            },
            TimeSpan.FromSeconds(60)
        );

        var chatName = frames.Last().ChatName;
        chatName.Should().NotBeNullOrWhiteSpace();

        var history = await AIChatHelper.CheckCountMessageInChatHistory(
            Client,
            chatId,
            userInfo.UserId,
            waitMessages: 3,
            timeoutSeconds: 30
        );

        history.Messages.Count.Should().Be(3);

        var devopsMessage = history.Messages
            .SingleOrDefault(m => m.Role == "developer");

        devopsMessage.Should().NotBeNull("pageContext must be stored as DevOps message in history");
        devopsMessage!.Text.Should().Contain("magicNumber");
        devopsMessage.Text.Should().Contain("777");
        devopsMessage.Text.Should().Contain("lesson-123");

        var regexMagic = new Regex(@"\b777\b");

        var combined = string.Concat(
            frames
                .Where(f => f.Stage == ChatStreamStage.Model)
                .Select(f => f.Delta)
        ) ?? string.Empty;

        combined.Should().MatchRegex(regexMagic);

        var lastAssistant = history.Messages.LastOrDefault(m => m.Role == "assistant");
        lastAssistant.Should().NotBeNull();
        (lastAssistant!.Text ?? string.Empty).Should().MatchRegex(regexMagic);
    }
}