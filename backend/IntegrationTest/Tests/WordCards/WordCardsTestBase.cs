using Azure.Core;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Constants;
using Manager.Models.Users;
using Manager.Models.WordCards;
using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.WordCards;

[Collection("Per-test user collection")]
public abstract class WordCardsTestBase(
    PerTestUserFixture perUserFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(perUserFixture.HttpFixture, outputHelper, signalRFixture)
{
    protected PerTestUserFixture PerUserFixture { get; } = perUserFixture;

    /// <summary>
    /// Creates a user (default role: student) and logs them in.
    /// </summary>
    protected Task<UserData> CreateUserAsync(
        string role = "student",
        string? email = null)
    {
        var parsedRole = Enum.TryParse<Role>(role, true, out var r) ? r : Role.Student;
        return PerUserFixture.CreateAndLoginAsync(parsedRole, email);
    }

    /// <summary>
    /// Creates a new word card for the specified user
    /// </summary>
    protected async Task<WordCard> CreateWordCardAsync(string hebrew, string english)
    {
        var request = new CreateWordCardRequest
        {
            Hebrew = hebrew,
            English = english
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.WordCards, request);
        response.EnsureSuccessStatusCode();

        var createdCard = await ReadAsJsonAsync<WordCard>(response);
        createdCard.Should().NotBeNull();
        return createdCard!;
    }

    /// <summary>
    /// Returns the list of word cards for the user
    /// </summary>
    protected async Task<List<WordCard>> GetWordCardsAsync()
    {
        var response = await Client.GetAsync($"{ApiRoutes.WordCards}");
        response.EnsureSuccessStatusCode();

        var cards = await ReadAsJsonAsync<List<WordCard>>(response);
        cards.Should().NotBeNull();
        return cards!;
    }

    /// <summary>
    /// Updates the learned status of a word card
    /// </summary>
    protected async Task<WordCardLearnedStatus> PatchLearnedStatusAsync(Guid cardId, bool isLearned)
    {
        var payload = new LearnedStatus
        {
            CardId = cardId,
            IsLearned = isLearned
        };

        var response = await Client.PatchAsJsonAsync($"{ApiRoutes.WordCardsUpdateLearnedStatus}", payload);
        response.EnsureSuccessStatusCode();

        var result = await ReadAsJsonAsync<WordCardLearnedStatus>(response);
        result.Should().NotBeNull();
        return result!;
    }

}
