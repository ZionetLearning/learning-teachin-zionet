using System.Text.Json;
using System.Text.RegularExpressions;
using DotQueue;
using Engine;
using Engine.Models.Chat;
using Engine.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Polly;

namespace EngineComponentTests;

[Collection("Kernel collection")]
public class ChatAiServiceTests
{
    private readonly TestKernelFixture _fx;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private readonly ChatAiService _aiService;
    private sealed class FakeRetryPolicyProvider : IRetryPolicy
    {
        private static readonly IAsyncPolicy _noOp = Policy.NoOpAsync();
        private static readonly IAsyncPolicy<ChatMessageContent> _noOpKernel =
            Policy.NoOpAsync<ChatMessageContent>();

        public IAsyncPolicy Create(QueueSettings settings, ILogger logger) => _noOp;

        public IAsyncPolicy<HttpResponseMessage> CreateHttpPolicy(ILogger logger) =>
            Policy.NoOpAsync<HttpResponseMessage>();

        public IAsyncPolicy<ChatMessageContent> CreateKernelPolicy(ILogger logger) =>
            _noOpKernel;
    }

    private sealed class FakeChatTitleService : IChatTitleService
    {
        public Task<string> GenerateAsync(ChatHistory history, CancellationToken ct = default)
            => Task.FromResult("New chat");
    }

    private static JsonElement JsonEl(string raw)
    {
        using var doc = JsonDocument.Parse(raw);
        return doc.RootElement.Clone();
    }

    private static JsonElement EmptyHistory() =>
        JsonEl("""{"messages":[]}""");

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
            new FakeChatTitleService(),
            Options.Create(_cacheOptions),
            new FakeRetryPolicyProvider());
    }

    [SkippableFact(DisplayName = "ProcessAsync: answer contains 4 or four")]
    public async Task ProcessAsync_Returns_Number4()
    {
        var userId = Guid.NewGuid();
        var request = new ChatAiServiseRequest
        {
            History = EmptyHistory(),
            UserMessage = "How much is 2 + 2?",
            ChatType = ChatType.Default,
            UserId = userId,
            RequestId = Guid.NewGuid().ToString("N"),
            ThreadId = Guid.NewGuid(),
            TtlSeconds = 60,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        var response = await _aiService.ChatHandlerAsync(request, CancellationToken.None);

        Assert.True(response.Status == ChatAnswerStatus.Ok);
        Assert.False(string.IsNullOrWhiteSpace(response?.Answer?.Content));

        var answerLower = response?.Answer?.Content.ToLowerInvariant();
        Assert.Matches(new Regex(@"\b4\b|four"), answerLower);
    }

    [SkippableFact(DisplayName = "ProcessAsync: history persists across calls")]
    public async Task ProcessAsync_Context_Persists()
    {
        var threadId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var userId = Guid.NewGuid();
        var userMesaage = "Remember the number forty-two.";
        var request1 = new ChatAiServiseRequest
        {
            History = EmptyHistory(),
            UserMessage = userMesaage,
            ChatType = ChatType.Default,
            UserId = userId,
            RequestId = Guid.NewGuid().ToString("N"),
            ThreadId = threadId,
            TtlSeconds = 60,
            SentAt = now,
        };
        var response1 = await _aiService.ChatHandlerAsync(request1, CancellationToken.None);

        Assert.True(response1.Status == ChatAnswerStatus.Ok);
        Assert.NotNull(response1.Answer);

        var request2 = new ChatAiServiseRequest
        {
            History = response1.UpdatedHistory,
            UserMessage = "What number did you remember?",
            ChatType = ChatType.Default,
            UserId = userId,
            RequestId = Guid.NewGuid().ToString("N"),
            ThreadId = threadId,
            TtlSeconds = 60,
            SentAt = now + 1,
        };

        var response2 = await _aiService.ChatHandlerAsync(request2, CancellationToken.None);

        Assert.True(response2.Status == ChatAnswerStatus.Ok);
        Assert.False(string.IsNullOrWhiteSpace(response2?.Answer?.Content));

        var answerLower = response2?.Answer?.Content.ToLowerInvariant();
        Assert.Matches(new Regex(@"\b42\b|forty[- ]?two"), answerLower);
    }

    [CollectionDefinition("Kernel collection")]
    public class KernelCollection : ICollectionFixture<TestKernelFixture> { }
}