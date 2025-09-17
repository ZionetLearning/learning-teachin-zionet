using DotQueue;
using Engine;
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

    public SentenceGeneratorServiceTests(TestKernelPluginFix fx)
    {
        _fx = fx;
        _sentenceService = new SentencesService(
            _fx.Kernel,
            NullLogger<SentencesService>.Instance);
    }

    [SkippableFact(DisplayName = "GenerateAsync: answer contains one sentence")]
    public async Task GenerateAsync_Returns_One_Sentence()
    {
        var userId = Guid.NewGuid();
        var request = new SentenceRequest
        {
            UserId = userId,
            Count = 1,
            Difficulty = Difficulty.medium,
            Nikud = false,
        };

        var response = await _sentenceService.GenerateAsync(request, [], CancellationToken.None);
        Assert.Equal(request.Count, response.Sentences.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task GenerateAsync_Returns_Requested_Count(int count)
    {
        var req = new SentenceRequest
        {
            UserId = Guid.NewGuid(),
            Count = count,
            Difficulty = Difficulty.medium,
            Nikud = false
        };

        var res = await _sentenceService.GenerateAsync(req, [], CancellationToken.None);

        Assert.Equal(count, res.Sentences.Count);
    }

    [CollectionDefinition("Plugins collection")]
    public class KernelCollection : ICollectionFixture<TestKernelPluginFix> { }
}