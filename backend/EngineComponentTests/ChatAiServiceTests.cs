using Engine.Models;
using Engine.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace EngineComponentTests;

[Collection("Kernel collection")]
public class ChatAiServiceTests
{
    private readonly TestKernelFixture _fx;

    public ChatAiServiceTests(TestKernelFixture fx)
    {
        _fx = fx;
    }
    [SkippableFact(DisplayName = "ProcessAsync: answer contains 4 or four")]
    public async Task ProcessAsync_Returns_Number4()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")),
        "No AzureOpenAI key - skip live test");

        // Arrange
        var service = new ChatAiService(_fx.Kernel, NullLogger<ChatAiService>.Instance);

        var req = new AiRequestModel
        {
            Id = Guid.NewGuid().ToString("N"),
            Question = "How much is 2 + 2?",
            TtlSeconds = 120,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ReplyToTopic = "ignored-in-test"
        };

        // Act
        var resp = await service.ProcessAsync(req, CancellationToken.None);

        // Assert
        resp.Status.Should().Be("ok");
        resp.Answer.Should().NotBeNullOrWhiteSpace();

        var answerLower = resp.Answer.ToLowerInvariant();
        answerLower.Should().MatchRegex(@"\b4\b|four");
    }

    [CollectionDefinition("Kernel collection")]
    public class KernelCollection : ICollectionFixture<TestKernelFixture>
    {
    }
}