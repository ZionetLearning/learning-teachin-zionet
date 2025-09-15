using System.Text.Json;
using DotQueue;
using Engine;
using Engine.Constants;
using Engine.Models.Sentences;
using Engine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Polly;

namespace EngineComponentTests;

[Collection("Plugins collection")]
public class SentenceGeneratorServiceTests
{
    private readonly TestKernelPluginFix _fx;
    private readonly SentencesService _sentenceService;
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

    public SentenceGeneratorServiceTests(TestKernelPluginFix fx)
    {
        _fx = fx;
        _sentenceService = new SentencesService(
            _fx.Kernel,
            NullLogger<SentencesService>.Instance);
    }

    [SkippableFact(DisplayName = "ProcessAsync: answer contains one sentence")]
    public async Task ProcessAsync_Returns_One_Sentence()
    {
        var userId = Guid.NewGuid();
        var request = new SentenceRequest
        {
            UserId = userId,
            Count = 1,
            Difficulty = Difficulty.medium,
            Nikud = false,
        };

        var response = await _sentenceService.GenerateAsync(request, CancellationToken.None);
        Assert.Equal(request.Count, response.Sentences.Count);
    }

    [CollectionDefinition("Plugins collection")]
    public class KernelCollection : ICollectionFixture<TestKernelPluginFix> { }
}