using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models.Notification;
using Manager.Models.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Models.Ai.Sentences;
using System.Net.Http.Json;
using System.Text.Json;
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

            await _shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);
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

            SentenceResponse res = JsonSerializer.Deserialize<SentenceResponse>(
                evtRaw.Payload.GetRawText(), options)!;

            res.Sentences.Count.Should().Be(count);

            Assert.All(res.Sentences, s =>
            {
                Assert.Equal(request.Difficulty.ToString(), s.Difficulty);
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

            await _shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);
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

            SplitSentenceResponse res = JsonSerializer.Deserialize<SplitSentenceResponse>(
                evtRaw.Payload.GetRawText(), options)!;

            res.Sentences.Count.Should().Be(count);

            Assert.All(res.Sentences, s =>
            {
                Assert.Equal(request.Difficulty.ToString(), s.Difficulty);
                Assert.Equal(request.Nikud, s.Nikud);
            });
        }

        [Fact(DisplayName = "Sentence generation may include user interests")]
        public async Task GenerateAsync_May_Include_Interest()
        {
            var testUser = _shared.UserFixture.TestUser;
            await _shared.GetAuthenticatedTokenAsync();
            await _shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);
            SignalRFixture.ClearReceivedMessages();

            // Update test user to have interests
            await Client.PutAsJsonAsync(ApiRoutes.UserById(testUser.UserId), 
                new UpdateUserModel
            {
                Role = Role.Student,
                Interests = ["food"]
            });

            var request = new SentenceRequestDto
            {
                Difficulty = Difficulty.easy,
                Nikud = true,
                Count = 5
            };

            var response = await PostAsJsonAsync(ApiRoutes.Sentences, request);
            response.EnsureSuccessStatusCode();

            var evt = await WaitForEventAsync(e => e.EventType == EventType.SentenceGeneration, TimeSpan.FromSeconds(20));
            evt.Should().NotBeNull("Expected a SignalR sentence event");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<SentenceResponse>(evt!.Event.Payload.GetRawText(), options)!;

            parsed.Sentences.Should().HaveCount(5);

            // Loosely validate interest appeared
            var keywords = new[] { "אוכל", "מסעדה", "מטבח", "בישול", "מאכל", "טבח" };
            var interestMatches = parsed.Sentences.Count(s => keywords.Any(k => s.Text.Contains(k)));

            // We dont assert because interest inclusion is probabilistic, so we log the outcome instead
            OutputHelper.WriteLine($"Interest match count: {interestMatches} / {parsed.Sentences.Count}");

            parsed.Sentences.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s.Text));
            parsed.Sentences.Should().OnlyContain(s => s.Text.Any(c => c >= 0x0590 && c <= 0x05FF), "Expected Hebrew characters");

        }

    }
}
