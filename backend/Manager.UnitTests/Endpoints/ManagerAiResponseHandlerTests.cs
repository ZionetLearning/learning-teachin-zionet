using System.Threading;
using System.Threading.Tasks;
using Manager.Endpoints;
using Manager.Models;
using Manager.Services;
using Moq;
using Xunit;

namespace Manager.UnitTests.Messaging;

public class ManagerAiResponseTests
{
    [Fact]
    public async Task Handle_Invalid_Model_Logs_Warning_And_Returns()
    {
        var ai = new Mock<IAiGatewayService>(MockBehavior.Strict);
        var log = new Mock<Microsoft.Extensions.Logging.ILogger<ManagerAiResponseHandler>>();
        var sut = new ManagerAiResponseHandler(ai.Object, log.Object);

        // invalid: empty strings will fail ValidationExtensions.TryValidate
        var msg = new AiResponseModel { Id = "", ThreadId = "", Answer = "" };

        await sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        ai.Verify(a => a.SaveAnswerAsync(It.IsAny<AiResponseModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Valid_Saves_Answer()
    {
        var ai = new Mock<IAiGatewayService>(MockBehavior.Strict);
        var log = new Mock<Microsoft.Extensions.Logging.ILogger<ManagerAiResponseHandler>>();
        var sut = new ManagerAiResponseHandler(ai.Object, log.Object);

        var msg = new AiResponseModel { Id = "1", ThreadId = "t", Answer = "a" };
        ai.Setup(a => a.SaveAnswerAsync(msg, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        ai.VerifyAll();
    }

    [Fact]
    public async Task Handle_When_Save_Fails_Rethrows()
    {
        var ai = new Mock<IAiGatewayService>(MockBehavior.Strict);
        var log = new Mock<Microsoft.Extensions.Logging.ILogger<ManagerAiResponseHandler>>();
        var sut = new ManagerAiResponseHandler(ai.Object, log.Object);

        var msg = new AiResponseModel { Id = "1", ThreadId = "t", Answer = "a" };
        ai.Setup(a => a.SaveAnswerAsync(msg, It.IsAny<CancellationToken>()))
          .ThrowsAsync(new System.Exception("store failed"));

        await Assert.ThrowsAsync<System.Exception>(() =>
            sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None));
    }
}
