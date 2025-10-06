using System.Text.Json;
using System.Text.RegularExpressions;
using DotQueue;
using Engine;
using Engine.Constants;
using Engine.Helpers;
using Engine.Models.Chat;
using Engine.Plugins;
using Engine.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Polly;

namespace EngineComponentTests;

[Collection("Kernel collection")]
public class ChatAiServiceTests
{
    private readonly TestKernelFixture _fx;
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
        public Task<string> GenerateTitleAsync(ChatHistory history, CancellationToken ct = default)
            => Task.FromResult("New chat");
    }

    private static JsonElement JsonEl(string raw)
    {
        using var doc = JsonDocument.Parse(raw);
        return doc.RootElement.Clone();
    }

    private static JsonElement EmptyHistory() =>
        JsonEl("""{"messages":[]}""");
    private sealed class SpyClock : IDateTimeProvider
    {
        private int _hits;
        public int Hits => _hits;
        public DateTimeOffset Fixed { get; }

        public SpyClock(DateTimeOffset fixedTime) => Fixed = fixedTime;

        public DateTimeOffset UtcNow
        {
            get
            {
                Interlocked.Increment(ref _hits);
                return Fixed;
            }
        }
    }

    private sealed class TimeInvocationSpy : IFunctionInvocationFilter
    {
        private int _count;
        public int Count => _count;

        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            if (string.Equals(context.Function.Name, PluginNames.CurrentTime, StringComparison.OrdinalIgnoreCase))
            {
                Interlocked.Increment(ref _count);
            }

            await next(context);
        }
    }

    public ChatAiServiceTests(TestKernelFixture fx)
    {
        _fx = fx;
        _aiService = new ChatAiService(
            _fx.Kernel,
            NullLogger<ChatAiService>.Instance,
            new FakeRetryPolicyProvider());
    }

    [SkippableFact(DisplayName = "ProcessAsync: answer contains 4 or four")]
    public async Task ProcessAsync_Returns_Number4()
    {
        var history = new ChatHistory();
        history.AddSystemMessage("You are a helpful assistant.");
        history.AddUserMessageNow("How much is 2 + 2?");
        var userId = Guid.NewGuid();
        var request = new ChatAiServiceRequest
        {
            History = history,
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
        var history = new ChatHistory();
        history.AddSystemMessage("You are a helpful assistant.");
        history.AddUserMessageNow("Remember the number forty-two.");

        var threadId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var userId = Guid.NewGuid();
        var request1 = new ChatAiServiceRequest
        {
            History = history,
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

        history.AddAssistantMessage(response1.Answer.Content);
        history.AddUserMessage("What number did you remember?", DateTimeOffset.UtcNow);

        var request2 = new ChatAiServiceRequest
        {
            History = history,
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

    [SkippableFact(DisplayName = "ProcessAsync: time question invokes TimePlugin")]
    public async Task ProcessAsync_TimePlugin_IsInvoked_OnTimeQuestion()
    {
        var fixedUtc = new DateTimeOffset(2025, 05, 01, 12, 34, 56, TimeSpan.Zero);
        var spyClock = new SpyClock(fixedUtc);
        var logger = NullLogger<TimePlugin>.Instance;

        var spyFilter = new TimeInvocationSpy();
        var pluginName = typeof(TimePlugin).ToPluginName();
        try
        {
            _fx.Kernel.Plugins.AddFromObject(new TimePlugin(spyClock, logger), pluginName);
        }
        catch
        {
            //if already added earlier - do not crash
        }

        _fx.Kernel.FunctionInvocationFilters.Add(spyFilter);

        var localCache = new MemoryCache(new MemoryCacheOptions());
        var ai = new ChatAiService(
            _fx.Kernel,
            NullLogger<ChatAiService>.Instance,
            new FakeRetryPolicyProvider());

        var history = new ChatHistory();
        history.AddSystemMessage("You are a helpful assistant.");
        history.AddUserMessageNow("What time is it now?");

        var request = new ChatAiServiceRequest
        {
            History = history,
            ChatType = ChatType.Default,
            UserId = Guid.NewGuid(),
            RequestId = Guid.NewGuid().ToString("N"),
            ThreadId = Guid.NewGuid(),
            TtlSeconds = 120,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        try
        {
            // Act
            var response = await ai.ChatHandlerAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(ChatAnswerStatus.Ok, response.Status);
            Assert.True(spyFilter.Count > 0, $"Expected plugin function {PluginNames.CurrentTime} to be invoked at least once.");
            Assert.True(spyClock.Hits > 0, "Expected IDateTimeProvider.UtcNow to be read.");

        }
        finally
        {
            _fx.Kernel.FunctionInvocationFilters.Remove(spyFilter);
        }
    }

    [CollectionDefinition("Kernel collection")]
    public class KernelCollection : ICollectionFixture<TestKernelFixture> { }
}