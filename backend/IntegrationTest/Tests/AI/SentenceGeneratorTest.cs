using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models.Ai.Sentences;
using IntegrationTests.Models.Notification;
using Manager.Models.Users;
using Models.Ai.Sentences;

using System.Text.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.AI
{

    [Collection("IntegrationTests")]
    public class SentenceGeneratorTests(
        HttpClientFixture clientFixture,
        ITestOutputHelper outputHelper,
        SignalRTestFixture signalRFixture
    ) : IntegrationTestBase(clientFixture, outputHelper, signalRFixture), IAsyncLifetime

    {
        public override async Task InitializeAsync()
        {
            await ClientFixture.LoginAsync(Role.Admin);
            await EnsureSignalRStartedAsync();
            SignalRFixture.ClearReceivedMessages();
        }

        [Theory]
        [InlineData(1, Difficulty.hard, false)]
        [InlineData(5, Difficulty.medium, false)]
        [InlineData(10, Difficulty.easy, true)]
        public async Task GenerateAsync_Returns_Requested_Count(int count, Difficulty difficulty, bool nikud)
        {
            var request = new SentenceRequest
            {
                UserId = Guid.NewGuid(),
                Difficulty = difficulty,
                Nikud = nikud,
                Count = count
            };

            await EnsureSignalRStartedAsync();
            SignalRFixture.ClearReceivedMessages();

            OutputHelper.WriteLine($"Generating sentences");

            var response = await PostAsJsonAsync(ApiRoutes.Sentences, request);
            response.EnsureSuccessStatusCode();

            var received = await WaitForEventAsync(
                n => n.EventType == EventType.SentenceGeneration,
                TimeSpan.FromSeconds(20)
            );
            received.Should().NotBeNull("Expected a SignalR notification");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var evtRaw = received.Event;

            // Backend sends List<AttemptedSentenceResult> not SentenceResponse
            var res = JsonSerializer.Deserialize<List<AttemptedSentenceResult>>(
                evtRaw.Payload.GetRawText(), options)!;

            res.Count.Should().Be(count);

            Assert.All(res, s =>
            {
                Assert.Equal(request.Difficulty.ToString(), s.Difficulty, ignoreCase: true);
                Assert.Equal(request.Nikud, s.Nikud);
            });
        }

        [Theory]
        [InlineData(1, Difficulty.hard, false)]
        [InlineData(5, Difficulty.medium, false)]
        [InlineData(10, Difficulty.easy, true)]
        public async Task GenerateSplitAsync_Returns_Requested_Count(int count, Difficulty difficulty, bool nikud)
        {
            var request = new SentenceRequest
            {
                UserId = Guid.NewGuid(),
                Difficulty = difficulty,
                Nikud = nikud,
                Count = count
            };

            await EnsureSignalRStartedAsync();
            SignalRFixture.ClearReceivedMessages();

            OutputHelper.WriteLine($"Generating sentences");

            var response = await PostAsJsonAsync(ApiRoutes.SplitSentences, request);
            response.EnsureSuccessStatusCode();

            var received = await WaitForEventAsync(
                n => n.EventType == EventType.SplitSentenceGeneration,
                TimeSpan.FromSeconds(20)
            );
            received.Should().NotBeNull("Expected a SignalR notification");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var evtRaw = received.Event;

            var res = JsonSerializer.Deserialize<List<AttemptedSentenceResult>>(
            evtRaw.Payload.GetRawText(), options)!;

            res.Count.Should().Be(count);


            Assert.All(res, s =>
            {
                Assert.Equal(request.Difficulty.ToString(), s.Difficulty, ignoreCase: true);
                Assert.Equal(request.Nikud, s.Nikud);
            });

        }
    }
}
