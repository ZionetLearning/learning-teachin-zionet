using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Models.Ai.Sentences;
using IntegrationTests.Models.Games;
using IntegrationTests.Models.Notification;
using Models.Ai.Sentences;
using Manager.Models.Chat;
using Manager.Models.Users;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;
using Manager.Models.UserGameConfiguration;


namespace IntegrationTests.Tests.AI;

[Collection("IntegrationTests")]
public class MistakeExplanationIntegrationTests(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : AiChatTestBase(httpClientFixture, outputHelper, signalRFixture)
{

    [Fact(DisplayName = "Complete mistake explanation flow test")]
    public async Task MistakeExplanation_CompleteFlow_ShouldExplainMistake()
    {
        // Arrange
        var userInfo = ClientFixture.GetUserInfo(Role.Admin);

        // Step 1: Generate split sentences for word order game
        OutputHelper.WriteLine("Step 1: Generating split sentences for word order game");
        var sentenceRequest = new SentenceRequest
        {
            UserId = userInfo.UserId,
            Difficulty = Difficulty.easy,
            Nikud = false,
            Count = 1
        };

        var sentenceResponse = await PostAsJsonAsync(ApiRoutes.SplitSentences, sentenceRequest);
        sentenceResponse.EnsureSuccessStatusCode();

        var sentenceEvent = await WaitForEventAsync(
            n => n.EventType == EventType.SplitSentenceGeneration,
            TimeSpan.FromSeconds(30)
        );
        sentenceEvent.Should().NotBeNull("Expected a SignalR notification for sentence generation");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var sentences = JsonSerializer.Deserialize<List<AttemptedSentenceResult>>(
            sentenceEvent.Event.Payload.GetRawText(), options)!;
        
        sentences.Should().NotBeEmpty("Should have generated at least one sentence");
        var sentence = sentences.First();
        
        OutputHelper.WriteLine($"Generated sentence: {sentence.Original}");
        OutputHelper.WriteLine($"Correct words: {string.Join(" ", sentence.Words)}");

        // Step 2: Submit a wrong answer attempt
        OutputHelper.WriteLine("Step 2: Submitting wrong answer attempt");
        
        // Create a deliberately wrong answer by shuffling the words incorrectly
        var wrongAnswer = sentence.Words.ToList();
        if (wrongAnswer.Count > 1)
        {
            // Reverse the order to make it wrong
            wrongAnswer.Reverse();
        }
        else
        {
            // If only one word, we'll add a fake word to make it wrong
            wrongAnswer.Add("שגוי");
        }

        var attemptRequest = new SubmitAttemptRequest
        {
            StudentId = userInfo.UserId,
            AttemptId = sentence.AttemptId,
            GivenAnswer = wrongAnswer
        };

        var attemptResponse = await PostAsJsonAsync(ApiRoutes.GamesAttempt, attemptRequest);
        attemptResponse.EnsureSuccessStatusCode();

        var attemptResult = await ReadAsJsonAsync<SubmitAttemptResponse>(attemptResponse);
        attemptResult.Should().NotBeNull();
        attemptResult!.Status.Should().Be("Failure", "The wrong answer should result in failure");
        
        OutputHelper.WriteLine($"Attempt failed as expected. Status: {attemptResult.Status}");
        OutputHelper.WriteLine($"Wrong answer submitted: {string.Join(" ", wrongAnswer)}");

        // Step 3: Request mistake explanation
        OutputHelper.WriteLine("Step 3: Requesting mistake explanation via chat");
        
        var threadId = Guid.NewGuid().ToString();
        var explainRequest = new ExplainMistakeRequest
        {
            AttemptId = sentence.AttemptId,
            ThreadId = threadId,
            GameType = GameName.WordOrder,
            ChatType = ChatType.ExplainMistake
        };

        var explainResponse = await PostAsJsonAsync(ApiRoutes.ChatMistakeExplanation, explainRequest);
        explainResponse.EnsureSuccessStatusCode();

        var doc = await explainResponse.Content.ReadFromJsonAsync<JsonElement>();
        doc.TryGetProperty("requestId", out var ridEl).Should().BeTrue();
        var requestId = ridEl.GetString();
        requestId.Should().NotBeNullOrWhiteSpace();

        OutputHelper.WriteLine($"Mistake explanation request sent. RequestId: {requestId}");

        // Step 4: Wait for streaming chat response
        OutputHelper.WriteLine("Step 4: Waiting for mistake explanation response");
        
        var (chatEvent, frames) = await WaitForChatResponseAsync(requestId!, TimeSpan.FromSeconds(60));
        
        frames.Should().NotBeNull();
        frames.Length.Should().BeGreaterThan(0, "Expected streaming frames for the mistake explanation");

        // Combine all the delta text from model stages to get the full explanation
        var explanation = string.Concat(frames
            .Where(f => f.Stage == ChatStreamStage.Model && !string.IsNullOrEmpty(f.Delta))
            .Select(f => f.Delta));

        explanation.Should().NotBeNullOrWhiteSpace("Should receive an explanation for the mistake");
        
        // The explanation should contain some reference to the correct order or mistake
        var shouldContainOneOf = new[] { "סדר", "נכון", "שגוי", "טעות", "correct", "order", "mistake", "wrong" };
        explanation.Should().ContainAny(shouldContainOneOf, 
            "Explanation should contain words related to word order or mistakes");

        OutputHelper.WriteLine($"Received explanation: {explanation.Substring(0, Math.Min(200, explanation.Length))}...");

        // Verify the chat response has the correct metadata
        var lastFrame = frames.Last();
        lastFrame.IsFinal.Should().BeTrue("Last frame should be marked as final");
        lastFrame.ThreadId.ToString().Should().Be(threadId, "Response should have correct thread ID");

        OutputHelper.WriteLine("Mistake explanation flow completed successfully");
    }
}