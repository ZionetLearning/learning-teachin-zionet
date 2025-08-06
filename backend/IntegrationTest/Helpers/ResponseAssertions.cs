using System.Net;
using FluentAssertions;

namespace IntegrationTests.Helpers;

public static class ResponseAssertions
{
    public static void ShouldBeOk(this HttpResponseMessage response) =>
        response.StatusCode.Should().Be(HttpStatusCode.OK);

    public static void ShouldBeAccepted(this HttpResponseMessage response) =>
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

    public static void ShouldBeBadRequest(this HttpResponseMessage response) =>
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    public static void ShouldBeNotFound(this HttpResponseMessage response) =>
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
