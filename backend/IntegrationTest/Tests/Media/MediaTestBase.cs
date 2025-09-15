using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models.Media;
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
    
    private JsonSerializerOptions JsonSerializationOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true
    };
    protected async Task<SpeechTokenResponse> GetSpeechTokenDataAsync()
    {
        // Calls the endpoint mapped in MediaEndpoints: GET /media-manager/speech/token
        var response = await Client.GetAsync("media-manager/speech/token");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadFromJsonAsync<SpeechTokenResponse>(JsonSerializationOptions);
        return responseContent;
    }
}