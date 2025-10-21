using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using IntegrationTests.Models.Games;
using Manager.Models.Users;
using Manager.Models.WordCards;
using Microsoft.CognitiveServices.Speech.Transcription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.WordCards;

    public class WordCardsIntegrationTests(
    PerTestUserFixture perUserFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : WordCardsTestBase(perUserFixture, outputHelper, signalRFixture), IAsyncLifetime
{

    [Fact(DisplayName = "POST /wordcards-manager - Can create and fetch card")]
    public async Task Can_Create_And_Fetch_Card()
    {
        // Create student and log in
        var user = await CreateUserAsync();

        var createdCard = await CreateWordCardAsync("שלום", "Hi");
        createdCard.IsLearned.Should().BeFalse();
        createdCard!.Hebrew.Should().Be("שלום");
        createdCard.English.Should().Be("Hi");
        createdCard.IsLearned.Should().BeFalse();

        // Fetch all cards
        var cards = await GetWordCardsAsync();

        cards.Should().HaveCount(1);

        var fetchedCard = cards.Single();
        fetchedCard.CardId.Should().Be(createdCard.CardId);
        fetchedCard.Hebrew.Should().Be(createdCard.Hebrew);
        fetchedCard.English.Should().Be(createdCard.English);
        fetchedCard.IsLearned.Should().Be(createdCard.IsLearned);
    }


    [Fact(DisplayName = "PATCH /wordcards-accessor/learned - Can update learned status of word card")]
    public async Task Can_Update_Learned_Status()
    {
        // Arrange
        var user = await CreateUserAsync();

        var created = await CreateWordCardAsync("תודה", "Thanks");
        created.IsLearned.Should().BeFalse();

        // Act
        var result = await PatchLearnedStatusAsync(created.CardId, true);

        // Assert
        result.Should().NotBeNull();
        result.CardId.Should().Be(created.CardId);
        result.IsLearned.Should().BeTrue();

        // Re-fetch and validate
        var cards = await GetWordCardsAsync();
        var updatedCard = cards.Single(c => c.CardId == created.CardId);
        updatedCard.IsLearned.Should().BeTrue();
    }


    [Fact(DisplayName = "PATCH /wordcards-accessor/learned - Updating nonexistent card returns 404")]
    public async Task Update_Nonexistent_Card_Should_Return_NotFound()
    {
        var user = await CreateUserAsync();
        var fakeCardId = Guid.NewGuid();

        var request = new LearnedStatus
        {
            CardId = fakeCardId,
            IsLearned = true
        };

        var response = await Client.PatchAsJsonAsync($"{ApiRoutes.WordCards}/learned", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact(DisplayName = "POST /wordcards-manager - Can create multiple cards and fetch them")]
    public async Task Can_Create_Multiple_Cards_And_Fetch_Them()
    {
        var user = await CreateUserAsync();

        var cardsToCreate = new List<(string Hebrew, string English)>
    {
        ("חתול", "Cat"),
        ("כלב", "Dog"),
        ("ספר", "Book")
    };

        var createdCards = new List<WordCard>();

        foreach (var (hebrew, english) in cardsToCreate)
        {
            var card = await CreateWordCardAsync(hebrew, english);
            createdCards.Add(card);
        }

        var fetchedCards = await GetWordCardsAsync();
        
        fetchedCards.Should().NotBeNull();
        fetchedCards!.Should().HaveCount(3);

        foreach (var created in createdCards)
        {
            fetchedCards.Should().ContainEquivalentOf(created);
        }
    }
}

