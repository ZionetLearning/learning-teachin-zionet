using Manager.Endpoints;
using Manager.Models.Chat;
using Manager.Models.Sentences;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Engine;
using Manager.Services.Clients.Engine.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ManagerUnitTests.Endpoints;

public class AiEndpointsTests
{
    [Fact(DisplayName = "GET /ai/chats/{userId} => 200 + body when chats exist")]
    public async Task GetChats_Returns_Ok_When_Chats_Exist()
    {
        var userId = Guid.NewGuid();
        var chats = new List<ChatSummary>
        {
            new ChatSummary
            {
                ChatId = Guid.NewGuid(),
                ChatName = "test chat",
                ChatType = "default",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        var accessor = new Mock<IAccessorClient>();
        accessor.Setup(a => a.GetChatsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chats);

        var logger = Mock.Of<ILogger<object>>();

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(AiEndpoints),
            "GetChatsAsync",
            userId,
            accessor.Object,
            logger,
            CancellationToken.None
        );

        var ok = Assert.IsAssignableFrom<IValueHttpResult>(result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("chats", out _));
    }

    [Fact(DisplayName = "GET /ai/chats/{userId} => 404 when no chats")]
    public async Task GetChats_Returns_NotFound_When_None()
    {
        var userId = Guid.NewGuid();

        var accessor = new Mock<IAccessorClient>();
        accessor.Setup(a => a.GetChatsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ChatSummary>());

        var logger = Mock.Of<ILogger<object>>();

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(AiEndpoints),
            "GetChatsAsync",
            userId,
            accessor.Object,
            logger,
            CancellationToken.None
        );

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, status.StatusCode);
    }

    [Fact(DisplayName = "GET /ai/chat/{chatId}/{userId} => 200 + body when history exists")]
    public async Task GetChatHistory_Returns_Ok_When_History_Exists()
    {
        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var history = new GetChatHistoryResponse
        {
            ChatId = chatId,
            Name = "Test Chat",
            ChatType = "default",
            Messages = new List<ChatHistoryMessageDto>
            {
                new() { Role = "user", Text = "msg1", CreatedAt = DateTimeOffset.UtcNow }
            }
        };

        var engine = new Mock<IEngineClient>();
        engine.Setup(e => e.GetHistoryChatAsync(chatId, userId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(history);

        var logger = Mock.Of<ILogger<object>>();

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(AiEndpoints),
            "GetChatHistoryAsync",
            chatId,
            userId,
            engine.Object,
            logger,
            CancellationToken.None
        );

        var ok = Assert.IsAssignableFrom<IValueHttpResult>(result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
        var root = doc.RootElement;
        Assert.Equal(chatId, root.GetProperty("ChatId").GetGuid());
    }

    [Fact(DisplayName = "POST /ai-manager/sentence => 200 when valid")]
    public async Task Sentence_Returns_Ok_When_Valid()
    {
        var request = new SentenceRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            Difficulty = Difficulty.Medium,
            Nikud = true,
            Count = 1
        };
        var dto = new SentenceRequestDto
        {
            Difficulty = request.Difficulty,
            Nikud = request.Nikud,
            Count = request.Count
        };

        var engine = new Mock<IEngineClient>();
        engine.Setup(e => e.GenerateSentenceAsync(request))
            .ReturnsAsync((true, "ok"));

        var logger = Mock.Of<ILogger<object>>();
        var httpContext = new DefaultHttpContext();

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, request.UserId.ToString()) },
            authenticationType: "TestAuth"
        );

        httpContext.User = new ClaimsPrincipal(identity);

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(AiEndpoints),
            "SentenceGenerateAsync",
            dto,
            engine.Object,
            logger,
            httpContext,
            CancellationToken.None
        );

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, status.StatusCode);
    }
}