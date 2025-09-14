using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models.Notification;
using Manager.Models.Sentences;

using Xunit.Abstractions;

namespace IntegrationTests.Tests.AI
{

    [Collection("Shared test collection")]
    public class SentenceGeneratorTests(
        SharedTestFixture sharedFixture,
        ITestOutputHelper outputHelper,
        SignalRTestFixture signalRFixture
    ) : IntegrationTestBase(sharedFixture.HttpFixture, outputHelper, signalRFixture), IAsyncLifetime

    {
        private readonly SharedTestFixture _shared = sharedFixture;

        [Fact(DisplayName = "POST /ai-manager/sentence")]
        public async Task GenerateSentences()
        {
            var request = new SentenceRequest
            {
                UserId = Guid.NewGuid(),
                Difficulty = Difficulty.medium,
                Nikud = true,
                Count = 1
            };

            await _shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);
            SignalRFixture.ClearReceivedMessages();

            OutputHelper.WriteLine($"Generating sentences");

            var response = await PostAsJsonAsync(ApiRoutes.Sentences, request);
            response.EnsureSuccessStatusCode();

            var received = await WaitForNotificationAsync(
                n => n.Type == NotificationType.Success,
                TimeSpan.FromSeconds(20)
            );
            received.Should().NotBeNull("Expected a SignalR notification");
        }
    }
}
