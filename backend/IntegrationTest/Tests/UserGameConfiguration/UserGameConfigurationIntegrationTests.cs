using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using System.Net;
using Xunit.Abstractions;
using Manager.Models.UserGameConfiguration;

namespace IntegrationTests.Tests.UserGameConfiguration;

[Collection("IntegrationTests")]
public class UserGameConfigurationIntegrationTests(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : UserGameConfigurationTestBase(httpClientFixture, outputHelper, signalRFixture)
{
    [Fact(DisplayName = "PUT /game-config-manager - Can save and fetch user game configuration")]
    public async Task Can_Save_And_Fetch_Game_Configuration()
    {
        var gameName = GameName.WordOrder;
        await SaveGameConfigAsync(gameName, "Hard", true, 4);

        var config = await GetGameConfigAsync(gameName);
        config.GameName.ToString().Should().Be(gameName.ToString());
        config.Difficulty.ToString().Should().Be("Hard");
        config.Nikud.Should().BeTrue();
        config.NumberOfSentences.Should().Be(4);
    }


    [Fact(DisplayName = "Full flow: save, fetch, delete, fetch (404) user game configuration")]
    public async Task Full_UserGameConfiguration_Flow_Works_Correctly()
    {
        var gameName = GameName.TypingPractice;

        // Save
        await SaveGameConfigAsync(gameName, "Medium", true, 3);

        // Fetch and assert
        var config = await GetGameConfigAsync(gameName);
        config.GameName.ToString().Should().Be(gameName.ToString());
        config.Difficulty.ToString().Should().Be("Medium");
        config.Nikud.Should().BeTrue();
        config.NumberOfSentences.Should().Be(3);

        // Delete
        var deleteResponse = await Client.DeleteAsync(ApiRoutes.GameConfigByName(gameName));
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Fetch again – should return 404
        var fetchAfterDelete = await Client.GetAsync(ApiRoutes.GameConfigByName(gameName));
        fetchAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}

