using Engine.Models;
using Engine.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.RegularExpressions;

namespace EngineComponentTests;

[Collection("Kernel collection")]
public class ChatAiServiceTests
{
    private readonly TestKernelFixture _fx;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private readonly ChatAiService _aiService;


    public ChatAiServiceTests(TestKernelFixture fx)
    {
        _fx = fx;

        _cache = new MemoryCache(new MemoryCacheOptions());
        _cacheOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
        _aiService = new ChatAiService(
            _fx.Kernel,
            NullLogger<ChatAiService>.Instance,
            _cache,
            _cacheOptions);
    }
    
    [SkippableFact(DisplayName = "ProcessAsync: answer contains 4 or four")]
    public async Task ProcessAsync_Returns_Number4()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var cacheOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };

        var request = new AiRequestModel
        {
            Id = Guid.NewGuid().ToString("N"),
            ThreadId = Guid.NewGuid().ToString("N"),
            Question = "How much is 2 + 2?",
            TtlSeconds = 120,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ReplyToTopic = "ignored-in-test"
        };

        var response = await _aiService.ProcessAsync(request, CancellationToken.None);

        Assert.True(string.IsNullOrEmpty(response.Status) || response.Status == "ok");
        Assert.False(string.IsNullOrWhiteSpace(response.Answer));

        var answerLower = response.Answer.ToLowerInvariant();
        Assert.Matches(new Regex(@"\b4\b|four"), answerLower);
    }

    [SkippableFact(DisplayName = "ProcessAsync: history persists across calls")]
    public async Task ProcessAsync_Context_Persists()
    {
        var threadId = Guid.NewGuid().ToString("N");
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var request1 = new AiRequestModel
        {
            Id = Guid.NewGuid().ToString("N"),
            ThreadId = threadId,
            Question = "Remember the number forty-two.",
            TtlSeconds = 120,
            SentAt = now,
            ReplyToTopic = "ignored"
        };
        var response1 = await _aiService.ProcessAsync(request1, CancellationToken.None);

        Assert.True(string.IsNullOrEmpty(response1.Status) || response1.Status == "ok");

        var request2 = new AiRequestModel
        {
            Id = Guid.NewGuid().ToString("N"),
            ThreadId = threadId,
            Question = "What number did you remember?",
            TtlSeconds = 120,
            SentAt = now + 1,
            ReplyToTopic = "ignored"
        };
        var response2 = await _aiService.ProcessAsync(request2, CancellationToken.None);

        Assert.True(string.IsNullOrEmpty(response2.Status) || response2.Status == "ok");
        Assert.False(string.IsNullOrWhiteSpace(response2.Answer));

        var answerLower = response2.Answer.ToLowerInvariant();
        Assert.Matches(new Regex(@"\b42\b|forty[- ]?two"), answerLower);
    }

    [CollectionDefinition("Kernel collection")]
    public class KernelCollection : ICollectionFixture<TestKernelFixture> { }
}