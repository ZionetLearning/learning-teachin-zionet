using Azure.AI.OpenAI;
using Engine.Constants.Chat;
using Engine.Models;
using Engine.Models.Prompts;
using Engine.Models.Sentences;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OpenAI.Chat;

namespace EngineComponentTests;

// Todo: fix all tests

public class SentenceGeneratorServiceTests
{
    private readonly Mock<AzureOpenAIClient> _azureClientMock;
    private readonly Mock<ChatClient> _chatClientMock;
    private readonly Mock<IAccessorClient> _accessorMock;
    private readonly SentencesService _sentenceService;

    public SentenceGeneratorServiceTests()
    {
        var settings = new AzureOpenAiSettings
        {
            DeploymentName = "gpt-4o",
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key"
        };
        var optionsMock = Options.Create(settings);

        _accessorMock = new Mock<IAccessorClient>();
        _accessorMock
            .Setup(x => x.GetPromptAsync(PromptsKeys.SentencesGenerateTemplate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PromptResponse
            {
                PromptKey = PromptsKeys.SentencesGenerateTemplate.Key,
                Content = "You are a sentence generator..."
            });

        _azureClientMock = new Mock<AzureOpenAIClient>();
        _chatClientMock = new Mock<ChatClient>();

        _azureClientMock
            .Setup(x => x.GetChatClient(settings.DeploymentName))
            .Returns(_chatClientMock.Object);

        _sentenceService = new SentencesService(
            _azureClientMock.Object,
            optionsMock,
            _accessorMock.Object,
            NullLogger<SentencesService>.Instance);
    }

    [SkippableFact(DisplayName = "GenerateAsync: answer contains one sentence")]
    public async Task GenerateAsync_Returns_One_Sentence()
    {
        var userId = Guid.NewGuid();
        var request = new SentenceRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            UserId = userId,
            Count = 1,
            Difficulty = Difficulty.Medium,
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
            RequestId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            Count = count,
            Difficulty = Difficulty.Medium,
            Nikud = false
        };

        var res = await _sentenceService.GenerateAsync(req, [], CancellationToken.None);

        Assert.Equal(count, res.Sentences.Count);
    }
}