using System.Text.Json;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Media;

public abstract class MediaTestBase(
    SharedTestFixture sharedFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(sharedFixture.HttpFixture, outputHelper, signalRFixture)
{
    protected SharedTestFixture Shared { get; } = sharedFixture;

    public override Task InitializeAsync() => SuiteInit.EnsureAsync(Shared, SignalRFixture, OutputHelper);

    protected async Task<string> GetSpeechTokenAsync()
    {
        // Calls the endpoint mapped in MediaEndpoints: GET /media-manager/speech/token
        var response = await Client.GetAsync("media-manager/speech/token");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        if (doc.RootElement.TryGetProperty("token", out var tokenEl) && tokenEl.ValueKind == JsonValueKind.String)
        {
            return tokenEl.GetString()!;
        }

        throw new InvalidOperationException("Response did not contain a 'token' field.");
    }
}
