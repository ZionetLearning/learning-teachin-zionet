using Manager.Common;
using Manager.Endpoints;
using Manager.Constants;
using Manager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace ManagerUnitTests.Endpoints;

public class AiEndpointsTests
{
    [Fact(DisplayName = "GET /ai/answer/{id} => 200 + body when answer exists")]
    public async Task Answer_Returns_Ok_When_Answer_Exists()
    {
        var id = "q-123";
        var answer = "hello";

        AiAnswerStore.Answers.Clear();
        AiAnswerStore.Answers[id] = answer;

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(AiEndpoints),
            "AnswerAsync",
            id,
            Mock.Of<ILogger<object>>()
        );

        var ok = Assert.IsAssignableFrom<IValueHttpResult>(result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
        var root = doc.RootElement;
        Assert.Equal(id, root.GetProperty("id").GetString());
        Assert.Equal(answer, root.GetProperty("answer").GetString());
    }

    [Fact(DisplayName = "GET /ai/answer/{id} => 404 when not ready")]
    public async Task Answer_Returns_NotFound_When_NotReady()
    {
        var id = "q-404";
        AiAnswerStore.Answers.Clear();

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(AiEndpoints),
            "AnswerAsync",
            id,
            Mock.Of<ILogger<object>>()
        );

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

        var dapr = new Mock<Dapr.Client.DaprClient>();
        dapr.Setup(d => d.InvokeBindingAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            null,
            default))
            .Returns(Task.CompletedTask);

        var options = Microsoft.Extensions.Options.Options.Create(new AiSettings
        {
            DefaultTtlSeconds = 60
        });

        var logger = Mock.Of<ILogger<object>>();

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(AiEndpoints),
            "QuestionAsync",
            dto,
            dapr.Object,
            options,
            logger
        );

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status202Accepted, status.StatusCode);

        var location = result.GetType().GetProperty("Location")?.GetValue(result) as string;
        Assert.Contains("/ai-manager/answer/", location);

        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(valueResult.Value));
        var root = doc.RootElement;

        Assert.Equal(dto.ThreadId, root.GetProperty("threadId").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("questionId").GetString()));
    }
}