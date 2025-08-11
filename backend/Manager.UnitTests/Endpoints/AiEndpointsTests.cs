using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Manager.UnitTests.TestHelpers;
using Manager.Services;
using Manager.Services.Clients;
using Moq;
using Xunit;

namespace Manager.UnitTests.Endpoints;

public class AiEndpointsTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;

    public AiEndpointsTests(CustomWebAppFactory factory) => _factory = factory;

    // ---------- GET /ai/answer/{id} ----------

    [Fact]
    public async Task Answer_Missing_Returns_404()
    {
        _factory.Mocks.AiGatewayService
            .Setup(a => a.GetAnswerAsync("id-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/ai/answer/id-123");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Answer_Found_Returns_200_With_Body()
    {
        _factory.Mocks.AiGatewayService
            .Setup(a => a.GetAnswerAsync("id-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("hello");

        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/ai/answer/id-1");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<dynamic>();
        string answer = (string)body.answer;
        answer.Should().Be("hello");
    }

    [Fact]
    public async Task Answer_When_Service_Throws_Returns_Problem()
    {
        _factory.Mocks.AiGatewayService
            .Setup(a => a.GetAnswerAsync("bad", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/ai/answer/bad");

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // ---------- POST /ai/question ----------

    [Fact]
    public async Task Question_InvalidModel_Returns_400()
    {
        var client = _factory.CreateClient();

        // Post JSON with an empty question to trigger validation failure
        var payload = new
        {
            id = "x",
            threadId = "t-1",
            question = "",            // invalid
            ttlSeconds = 60,
            replyToTopic = "manager-ai"
        };

        var resp = await client.PostAsJsonAsync("/ai/question", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Question_Valid_Returns_202_With_Location()
    {
        _factory.Mocks.AiGatewayService
            .Setup(a => a.SendQuestionAsync("thread-1", "hey", It.IsAny<CancellationToken>()))
            .ReturnsAsync("qid-1");

        var client = _factory.CreateClient();

        // Use anonymous object so we don't rely on DTO constructors
        var payload = new
        {
            id = "ignored",
            threadId = "thread-1",
            question = "hey",
            ttlSeconds = 60,
            replyToTopic = "manager-ai"
        };

        var resp = await client.PostAsJsonAsync("/ai/question", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.Accepted);
        resp.Headers.Location!.ToString().Should().Be("/ai/answer/qid-1");
    }

    [Fact]
    public async Task Question_When_Service_Throws_Returns_Problem()
    {
        _factory.Mocks.AiGatewayService
            .Setup(a => a.SendQuestionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("send failed"));

        var client = _factory.CreateClient();

        var payload = new { id = "x", threadId = "t", question = "q", ttlSeconds = 30, replyToTopic = "manager-ai" };
        var resp = await client.PostAsJsonAsync("/ai/question", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // ---------- POST /chat ----------

    [Fact]
    public async Task Chat_Missing_UserMessage_Returns_400()
    {
        var client = _factory.CreateClient();

        // Minimal JSON to trigger the guard clause
        var resp = await client.PostAsJsonAsync("/chat", new { threadId = "t", userMessage = "" });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Chat_Success_Returns_200_With_Response()
    {
        _factory.Mocks.EngineClient
            .Setup(e => e.ChatAsync(It.IsAny<Manager.Models.ChatRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Manager.Models.ChatResponseDto("t", "ok"));

        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/chat", new { threadId = "t", userMessage = "hi" });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<Manager.Models.ChatResponseDto>();
        body!.AssistantMessage.Should().Be("ok");
    }

    [Fact]
    public async Task Chat_When_Engine_Throws_Returns_Problem()
    {
        _factory.Mocks.EngineClient
            .Setup(e => e.ChatAsync(It.IsAny<Manager.Models.ChatRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("engine down"));

        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/chat", new { threadId = "t", userMessage = "hi" });

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}