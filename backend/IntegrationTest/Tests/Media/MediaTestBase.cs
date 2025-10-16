using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models.Media;
using Manager.Models.Users;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Media;

public abstract class MediaTestBase(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBaseClientFixture(httpClientFixture, outputHelper, signalRFixture)
{
    public override async Task InitializeAsync()
    {
        // Login as Admin to get access token
        await ClientFixture.LoginAsync(Role.Admin);
        
        // Start SignalR with the authenticated token
        await EnsureSignalRStartedAsync();
        
        // Clear any previous messages
        SignalRFixture.ClearReceivedMessages();
    }
    
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