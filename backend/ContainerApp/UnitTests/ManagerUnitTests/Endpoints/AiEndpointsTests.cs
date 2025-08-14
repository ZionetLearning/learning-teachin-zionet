using Manager.Endpoints;
using Manager.Models;
using Manager.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text.Json;

namespace ManagerUnitTests.Endpoints;
public class AiEndpointsTests
{
    private readonly Mock<IAiGatewayService> _ai = new(MockBehavior.Strict);

    [Fact(DisplayName = "GET /ai/answer/{id} => 200 + body when answer exists")]
    public async Task Answer_Returns_Ok_When_Answer_Exists()
    {
        var id = "q-123";
        _ai.Setup(s => s.GetAnswerAsync(id, It.IsAny<CancellationToken>()))
           .ReturnsAsync("hello");

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(AiEndpoints),
            "AnswerAsync",
            id,
            _ai.Object);

        var ok = Assert.IsAssignableFrom<IValueHttpResult>(result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
        var root = doc.RootElement;
        Assert.Equal(id, root.GetProperty("id").GetString());
        Assert.Equal("hello", root.GetProperty("answer").GetString());
    }

    [Fact(DisplayName = "GET /ai/answer/{id} => 404 when not ready")]
    public async Task Answer_Returns_NotFound_When_NotReady()
    {
        var id = "q-404";
        _ai.Setup(s => s.GetAnswerAsync(id, It.IsAny<CancellationToken>()))
           .ReturnsAsync((string?)null);

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(AiEndpoints),
            "AnswerAsync",
            id,
            _ai.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, status.StatusCode);
    }
    [Fact(DisplayName = "POST /ai/question => 202 + location when valid")]
    public async Task Question_Returns_Accepted_When_Valid()
    {
        var dto = new AiRequestModel
        {
            ThreadId = Guid.NewGuid().ToString("N"),
            Question = "How are you?"
        };

        _ai.Setup(s => s.SendQuestionAsync(dto.ThreadId, dto.Question, It.IsAny<CancellationToken>()))
           .ReturnsAsync("new-id-1");

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(AiEndpoints),
            "QuestionAsync",
            dto,
            _ai.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status202Accepted, status.StatusCode);

        var location = result.GetType().GetProperty("Location")?.GetValue(result) as string;
        Assert.Equal("/ai/answer/new-id-1", location);

        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(valueResult.Value));
        var root = doc.RootElement;
        Assert.Equal("new-id-1", root.GetProperty("questionId").GetString());
        Assert.Equal(dto.ThreadId, root.GetProperty("threadId").GetString());
    }
}
