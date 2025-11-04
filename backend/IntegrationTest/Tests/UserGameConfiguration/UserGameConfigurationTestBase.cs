using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Models.UserGameConfiguration;
using Manager.Models.Users;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.UserGameConfiguration;

[Collection("IntegrationTests")]
public abstract class UserGameConfigurationTestBase(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(httpClientFixture, outputHelper, signalRFixture)
{
    public override async Task InitializeAsync()
    {
        await ClientFixture.LoginAsync(Role.Student);
        await EnsureSignalRStartedAsync();
        SignalRFixture.ClearReceivedMessages();
    }

    /// <summary>
    /// Saves a game configuration for the current user via PUT.
    /// </summary>
    protected async Task<HttpResponseMessage> SaveGameConfigAsync(string gameName, string difficulty = "Medium", bool nikud = true, int numberOfSentences = 3)
    {
        var payload = new
        {
            gameName,
            difficulty,
            nikud,
            numberOfSentences
        };

        return await Client.PutAsJsonAsync(ApiRoutes.GameConfig, payload);
    }

    /// <summary>
    /// Fetches the game configuration for the current user via GET.
    /// </summary>
    protected async Task<UserNewGameConfig> GetGameConfigAsync(string gameName)
    {
        var response = await Client.GetAsync(ApiRoutes.GameConfigByName(gameName));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var config = await response.Content.ReadFromJsonAsync<UserNewGameConfig>();
        config.Should().NotBeNull();
        return config!;
    }

    //public override async ValueTask DisposeAsync()
    //{
    //    await base.DisposeAsync();

    //    using var scope = ClientFixture.Services.CreateScope();
    //    var db = scope.ServiceProvider.GetRequiredService<AccessorDbContext>();

    //    db.UserGameConfigs.RemoveRange(db.UserGameConfigs);
    //    await db.SaveChangesAsync();
    //}
}
