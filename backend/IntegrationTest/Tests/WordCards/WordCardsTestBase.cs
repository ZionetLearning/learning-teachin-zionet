using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Models.WordCards.Requests;
using Manager.Models.WordCards.Responses;
using Manager.Models.Users;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.WordCards;

[Collection("IntegrationTests")]
public abstract class WordCardsTestBase(
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
    /// Creates a new word card for the specified user
    /// </summary>
    protected async Task<CreateWordCardResponse> CreateWordCardAsync(string hebrew, string english)
    {
        var request = new CreateWordCardRequest
        {
            Hebrew = hebrew,
            English = english
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.WordCards, request);
        response.EnsureSuccessStatusCode();

        var createdCard = await ReadAsJsonAsync<CreateWordCardResponse>(response);
        createdCard.Should().NotBeNull();
        return createdCard!;
    }

    /// <summary>
    /// Returns the list of word cards for the user
    /// </summary>
    protected async Task<List<GetWordCardsResponse>> GetWordCardsAsync()
    {
        var response = await Client.GetAsync($"{ApiRoutes.WordCards}");
        response.EnsureSuccessStatusCode();

        var cards = await ReadAsJsonAsync<List<GetWordCardsResponse>>(response);
        cards.Should().NotBeNull();
        return cards!;
    }

    /// <summary>
    /// Updates the learned status of a word card
    /// </summary>
    protected async Task<UpdateLearnedStatusResponse> PatchLearnedStatusAsync(Guid cardId, bool isLearned)
    {
        var payload = new UpdateLearnedStatusRequest
        {
            CardId = cardId,
            IsLearned = isLearned
        };

        var response = await Client.PatchAsJsonAsync($"{ApiRoutes.WordCardsUpdateLearnedStatus}", payload);
        response.EnsureSuccessStatusCode();

        var result = await ReadAsJsonAsync<UpdateLearnedStatusResponse>(response);
        result.Should().NotBeNull();
        return result!;
    }

}
