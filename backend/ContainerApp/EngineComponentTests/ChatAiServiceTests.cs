using Engine.Models;
using Engine.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.RegularExpressions;

namespace EngineComponentTests;

[Collection("Kernel collection")]
public class ChatAiServiceTests
{
    private readonly TestKernelFixture _fx;

    public ChatAiServiceTests(TestKernelFixture fx)
    {
        this._fx = fx;
    }
    [SkippableFact(DisplayName = "ProcessAsync: answer contains 4 or four")]
    public async Task ProcessAsync_Returns_Number4()
    {
        var service = new ChatAiService(this._fx.Kernel, NullLogger<ChatAiService>.Instance);

        var req = new AiRequestModel
        {
            Id = Guid.NewGuid().ToString("N"),
            Question = "How much is 2 + 2?",
            TtlSeconds = 120,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ReplyToTopic = "ignored-in-test"
        };

        var resp = await service.ProcessAsync(req, CancellationToken.None);

        Assert.Equal("ok", resp.Status);
        Assert.False(string.IsNullOrWhiteSpace(resp.Answer));

        var answerLower = resp.Answer.ToLowerInvariant();
        Assert.Matches(new Regex(@"\b4\b|four"), answerLower);
    }

    [CollectionDefinition("Kernel collection")]
    public class KernelCollection : ICollectionFixture<TestKernelFixture>
    {
    }
}