using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using Manager.Models.WordCards;
using System.Net;
using System.Net.Http.Json;
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
        var gameName = "WordOrder";
        await SaveGameConfigAsync(gameName, "Hard", true, 4);

        var config = await GetGameConfigAsync(gameName);
        config.GameName.ToString().Should().Be(gameName);
        config.Difficulty.ToString().Should().Be("Hard");
        config.Nikud.Should().BeTrue();
        config.NumberOfSentences.Should().Be(4);
    }

    [Fact(DisplayName = "GET /game-config-manager/{gameName} - Invalid enum value returns 400 Bad Request")]
    public async Task GetGameConfig_With_Invalid_GameName_Returns_BadRequest()
    {
        var invalidGameName = "invalidGameName";

        var response = await Client.GetAsync(ApiRoutes.GameConfigByName(invalidGameName));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact(DisplayName = "Full flow: save, fetch, delete, fetch (404) user game configuration")]
    public async Task Full_UserGameConfiguration_Flow_Works_Correctly()
    {
        var gameName = "WordOrder";

        // Save
        await SaveGameConfigAsync(gameName, "Medium", true, 3);

        // Fetch and assert
        var config = await GetGameConfigAsync(gameName);
        config.GameName.ToString().Should().Be(gameName);
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

