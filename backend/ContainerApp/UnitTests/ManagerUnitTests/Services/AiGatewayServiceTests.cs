using Dapr.Client;
using FluentAssertions;
using Manager.Constants;
using Manager.Models;
using Manager.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

public class AiGatewayServiceTests
{
    private static IOptions<AiSettings> Opt(int ttl = 60)
        => Options.Create(new AiSettings { DefaultTtlSeconds = ttl });

    [Fact]
    public async Task SendQuestionAsync_When_Dapr_Throws_Rethrows()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new AiGatewayService(dapr.Object,
                                        new Mock<ILogger<AiGatewayService>>().Object,
                                        Opt());

        dapr.Setup(d => d.InvokeBindingAsync(
                QueueNames.EngineQueue,
                "create",
                It.IsAny<AiRequestModel>(),
                It.IsAny<IReadOnlyDictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("boom"));

        await FluentActions.Invoking(() =>
                sut.SendQuestionAsync("t", "q", CancellationToken.None))
            .Should().ThrowAsync<System.Exception>();
    }

    [Fact]
    public async Task SaveAnswer_And_GetAnswer_Roundtrip_Works()
    {
        var dapr = new Mock<DaprClient>();
        var sut = new AiGatewayService(dapr.Object,
                                        new Mock<ILogger<AiGatewayService>>().Object,
                                        Opt());

        var resp = new AiResponseModel { Id = "x-1", ThreadId = "t-1", Answer = "42" };

        await sut.SaveAnswerAsync(resp, CancellationToken.None);
        var ans = await sut.GetAnswerAsync("x-1", CancellationToken.None);

        ans.Should().Be("42");
    }

    [Fact]
    public async Task GetAnswer_When_Missing_ReturnsNull()
    {
        var sut = new AiGatewayService(new Mock<DaprClient>().Object,
                                       new Mock<ILogger<AiGatewayService>>().Object,
                                       Opt());

        var ans = await sut.GetAnswerAsync("missing", CancellationToken.None);

        ans.Should().BeNull();
    }

    [Theory]
    [InlineData(true, "dapr")]
    [InlineData(false, "options")]
    public void Constructor_With_Nulls_Throws(bool nullOptions, string _)
    {
        var dapr = new Mock<DaprClient>().Object;
        var log = new Mock<ILogger<AiGatewayService>>().Object;

        if (nullOptions)
        {
            // options is required
            Assert.Throws<ArgumentNullException>(() =>
                new AiGatewayService(dapr, log, (IOptions<AiSettings>)null!));
        }
        else
        {
            // dapr is required (also covers logger via a second assert)
            Assert.Throws<ArgumentNullException>(() =>
                new AiGatewayService(null!, log, Opt()));
            Assert.Throws<ArgumentNullException>(() =>
                new AiGatewayService(dapr, null!, Opt()));
        }
    }
}
