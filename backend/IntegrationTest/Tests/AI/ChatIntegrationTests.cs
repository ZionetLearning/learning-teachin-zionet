using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using IntegrationTests.Models;
using IntegrationTests.Models.Notification;
using Manager.Models.Chat;
using System.Net.Http.Json;
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
        await _shared.GetAuthenticatedTokenAsync();

        await _shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);
    }

    [Fact(DisplayName = "Chat AI integration test")]
    public async Task Post_new_chat()
    {
        var user = _shared.UserFixture.TestUser;

        var chatId1 = Guid.NewGuid();

        var chatRequest1 = new ChatRequest
        {
            ThreadId = chatId1.ToString(),
            UserId = user.UserId.ToString(),
            UserMessage = "Remember number 42",
            ChatType = ChatType.Default

        };

        var chatMessage1 = TestDataHelper.CreateFixedIdTask(1001);
        var second = TestDataHelper.CreateFixedIdTask(1001); // same Id

        // 1) POST first
        var r1 = await Client.PostAsJsonAsync(AiRoutes.PostNewMessage, chatRequest1);
        r1.ShouldBeAccepted();

        // Prefer notification, but don't fail if it doesn't arrive in time
        var received = await TryWaitForNotificationAsync(
            n => n.Type == NotificationType.Success && n.Message.Contains(" "),
            TimeSpan.FromSeconds(20)
        );

        if (received is null)
            OutputHelper.WriteLine("No SignalR notification within timeout; proceeding via HTTP polling.");

        // 2) Confirm the first write happened (ground truth)
        var chatHistoryAfterRequest1 = await AIChatHelper.CheckCountMessageInChatHistory(Client, chatId1, user.UserId, 3, timeoutSeconds: 60);

        chatHistoryAfterRequest1.Messages.Count.Should().Be(3);

        //// 3) POST duplicate (same Id) — Manager still returns 202 Accepted
        //var r2 = await Client.PostAsJsonAsync(ApiRoutes.Task, second);
        //r2.ShouldBeAccepted();

        //// 4) Confirm nothing changed
        //var after = await TaskUpdateHelper.WaitForTaskByIdAsync(Client, first.Id, timeoutSeconds: 10);
        //after.Name.Should().Be(first.Name);
        //after.Payload.Should().Be(first.Payload);
    }
}