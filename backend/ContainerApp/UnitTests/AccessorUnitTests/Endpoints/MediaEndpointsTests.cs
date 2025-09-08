using System.Net;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests.Endpoints;

public class MediaEndpointsTests
{
    private static Mock<ISpeechService> Svc() => new(MockBehavior.Strict);

    [Fact]
    public async Task GetSpeechToken_ReturnsOk_WithToken()
    {
        var svc = Svc();

        const string token = "abc123";
        svc.Setup(s => s.GetSpeechTokenAsync(CancellationToken.None)).ReturnsAsync(token);

        var result = await MediaEndpoints.GetSpeechTokenAsync(svc.Object, CancellationToken.None);

        var ok = result.Should().BeOfType<Ok<string>>().Subject;
        ok.Value.Should().Be(token);
        svc.VerifyAll();
    }

    [Fact]
    public async Task GetSpeechToken_Returns499_OnCancellation()
    {
        var svc = Svc();
        svc.Setup(s => s.GetSpeechTokenAsync(CancellationToken.None))
           .ThrowsAsync(new OperationCanceledException());

        var result = await MediaEndpoints.GetSpeechTokenAsync(svc.Object, CancellationToken.None);

        var status = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        status.StatusCode.Should().Be(499);
        svc.VerifyAll();
    }

    [Fact]
    public async Task GetSpeechToken_Returns500_OnInvalidOperation()
    {
        var svc = Svc();
        svc.Setup(s => s.GetSpeechTokenAsync(CancellationToken.None))
           .ThrowsAsync(new InvalidOperationException("Speech service not configured"));

        var result = await MediaEndpoints.GetSpeechTokenAsync(svc.Object, CancellationToken.None);

        result.Should().BeOfType<ProblemHttpResult>();
        // Optionally check message content
        // But without executing IResult, we just assert type here as in style of first test
        svc.VerifyAll();
    }

    [Fact]
    public async Task GetSpeechToken_Returns502_OnHttpRequestException()
    {
        var svc = Svc();
        svc.Setup(s => s.GetSpeechTokenAsync(CancellationToken.None))
           .ThrowsAsync(new HttpRequestException("Bad Gateway from Azure"));

        var result = await MediaEndpoints.GetSpeechTokenAsync(svc.Object, CancellationToken.None);

        var status = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        status.StatusCode.Should().Be((int)HttpStatusCode.BadGateway);
        svc.VerifyAll();
    }

    [Fact]
    public async Task GetSpeechToken_Returns500_OnGenericException()
    {
        var svc = Svc();
        svc.Setup(s => s.GetSpeechTokenAsync(CancellationToken.None))
           .ThrowsAsync(new Exception("boom"));

        var result = await MediaEndpoints.GetSpeechTokenAsync(svc.Object, CancellationToken.None);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }
}
