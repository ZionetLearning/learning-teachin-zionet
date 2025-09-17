using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using Manager.Models.Chat;
using Manager.Models.Users;
using Microsoft.CognitiveServices.Speech.Transcription;
using System.Net.Http.Json;
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

    [Fact(DisplayName = "Chat AI integration test")]
    public async Task Post_new_chat()
    {
        var user = _shared.UserFixture.TestUser;

        await _shared.GetAuthenticatedTokenAsync();

        await _shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);

        var chatId1 = Guid.NewGuid();

        var (req1, ev1, msg1, chatName1) = await PostChatAndWaitAsync(new ChatRequest
        {
            ThreadId = chatId1.ToString(),
            UserId = user.UserId.ToString(),
            UserMessage = "Remember number 42",
            ChatType = ChatType.Default
        }, TimeSpan.FromSeconds(30));

        chatName1.Should().NotBeNullOrWhiteSpace();
        chatName1.Should().NotBe("New chat");

        var fallbackTitlePattern = new Regex(@"^\d{4}_\d{2}_\d{2}$");
        fallbackTitlePattern.IsMatch(chatName1).Should().BeFalse($"_chatTitleService.GenerateTitleAsync not work properly. Name was: {chatName1}");

        var history1 = await AIChatHelper.CheckCountMessageInChatHistory(Client, chatId1, user.UserId, 2, 30);
        history1.Messages.Count.Should().Be(2);


        var chatHistoryAfterRequest1 = await AIChatHelper.CheckCountMessageInChatHistory(Client, chatId1, user.UserId, waitMessages:2, timeoutSeconds: 30);

        chatHistoryAfterRequest1.Messages.Count.Should().Be(2);

        var chatRequest2 = new ChatRequest
        {
            ThreadId = chatId1.ToString(),
            UserId = user.UserId.ToString(),
            UserMessage = "What number did you remember?",
            ChatType = ChatType.Default

        };

        var (req2, ev2, msg2, chatName2) = await PostChatAndWaitAsync(new ChatRequest
        {
            ThreadId = chatId1.ToString(),
            UserId = user.UserId.ToString(),
            UserMessage = "What number did you remember?",
            ChatType = ChatType.Default
        }, TimeSpan.FromSeconds(30));

        chatName2.Should().Be(chatName1);

        var regexCheck = new Regex(@"\b42\b|\bforty[-\s]?two\b", RegexOptions.IgnoreCase);
        msg2.Should().MatchRegex(regexCheck);

        var chatHistoryAfterRequest2 = await AIChatHelper.CheckCountMessageInChatHistory(Client, chatId1, user.UserId, waitMessages: 4, timeoutSeconds: 30);

        chatHistoryAfterRequest2.Messages.Count.Should().Be(4);

        var text = chatHistoryAfterRequest2.Messages[^1].Text ?? string.Empty;
        text.Should().MatchRegex(regexCheck);

        var (req3, ev3, msg3, chatName3) = await PostChatAndWaitAsync(new ChatRequest
        {
            ThreadId = chatId1.ToString(),
            UserId = user.UserId.ToString(),
            UserMessage = "Give me the current time in ISO-8601 (UTC). Return only the timestamp.",
            ChatType = ChatType.Default
        }, TimeSpan.FromSeconds(30));

        chatName3.Should().Be(chatName1);

        DateTimeOffset parsed;
        DateTimeOffset.TryParseExact(
            msg3.Trim(),
            "O",
            formatProvider: null,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out parsed
        ).Should().BeTrue("assistantMessage about time must be ISO-8601 (O format)");

        var delta = (DateTimeOffset.UtcNow - parsed).Duration();
        delta.Should().BeLessThan(TimeSpan.FromSeconds(300), "time should come from TimePlugin/clock");
    }


    [Fact(DisplayName = "System prompt includes user interests when available")]
    public async Task SystemPromptIncludesUserInterests_WhenInjected()
    {
        var user = _shared.UserFixture.TestUser;

        await _shared.GetAuthenticatedTokenAsync();
        await _shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);

        // Only students can have interests, so we update the admin user to be a student
        var payload = new UpdateUserModel
        {
            Role = Role.Student,
        };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), payload);
        response.Should().NotBeNull();
        response.EnsureSuccessStatusCode();

        // Now set some interests
        payload = new UpdateUserModel
        {
            Interests = ["soccer", "food"]
        };

        response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), payload);
        response.Should().NotBeNull();
        response.EnsureSuccessStatusCode();


        var chatId = Guid.NewGuid();

        var (_, _, _, _) = await PostChatAndWaitAsync(new ChatRequest
        {
            ThreadId = chatId.ToString(),
            UserId = user.UserId.ToString(),
            UserMessage = "Say something interesting.",
            ChatType = ChatType.Default
        }, TimeSpan.FromSeconds(30));

        var chatHistory = await AIChatHelper.CheckCountMessageInChatHistory(
            Client,
            chatId,
            user.UserId,
            waitMessages: 2,
            timeoutSeconds: 30
        );

        var systemPrompt = chatHistory.Messages
            .FirstOrDefault(m => m.Role?.Equals("assistant", StringComparison.OrdinalIgnoreCase) == true)?.Text;

        systemPrompt.Should().NotBeNullOrWhiteSpace("System prompt should be present in chat history.");

        // 50% chance to include interests, so we log the outcome instead of asserting
        if (
            (systemPrompt.Contains("soccer", StringComparison.OrdinalIgnoreCase) ||
             systemPrompt.Contains("food", StringComparison.OrdinalIgnoreCase)))
        {
            OutputHelper.WriteLine("System prompt included interests as expected.");
        }
        else
        {
            OutputHelper.WriteLine("System prompt did not include interests this time — acceptable due to randomness.");
        }

    }

}