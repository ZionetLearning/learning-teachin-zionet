using Manager.Models.WordCards.Requests;
using Manager.Models.WordCards.Responses;
using Manager.Services.Clients.Accessor.Models.WordCards;

namespace Manager.Mapping;

/// <summary>
/// Provides mapping methods between frontend models and accessor models for WordCards domain
/// </summary>
public static class WordCardsMapper
{
    #region GetWordCards Mappings

    /// <summary>
    /// Maps Accessor GetWordCardsAccessorResponse to frontend GetWordCardsResponse
    /// </summary>
    public static GetWordCardsResponse ToFront(this GetWordCardsAccessorResponse accessorResponse)
    {
        return new GetWordCardsResponse
        {
            WordCards = accessorResponse.WordCards.Select(wc => new WordCardDto
            {
                CardId = wc.CardId,
                Hebrew = wc.Hebrew,
                English = wc.English,
                IsLearned = wc.IsLearned,
                Explanation = wc.Explanation
            }).ToList()
        };
    }

    #endregion

    #region CreateWordCard Mappings

    /// <summary>
    /// Maps frontend CreateWordCardRequest to Accessor request
    /// </summary>
    public static CreateWordCardAccessorRequest ToAccessor(this CreateWordCardRequest request, Guid userId)
    {
        return new CreateWordCardAccessorRequest
        {
            UserId = userId,
            Hebrew = request.Hebrew,
            English = request.English,
            Explanation = request.Explanation
        };
    }

    /// <summary>
    /// Maps Accessor CreateWordCardAccessorResponse to frontend CreateWordCardResponse
    /// </summary>
    public static CreateWordCardResponse ToFront(this CreateWordCardAccessorResponse accessorResponse)
    {
        return new CreateWordCardResponse
        {
            CardId = accessorResponse.CardId,
            Hebrew = accessorResponse.Hebrew,
            English = accessorResponse.English,
            IsLearned = accessorResponse.IsLearned,
            Explanation = accessorResponse.Explanation
        };
    }

    #endregion

    #region UpdateLearnedStatus Mappings

    /// <summary>
    /// Maps frontend UpdateLearnedStatusRequest to Accessor request
    /// </summary>
    public static UpdateLearnedStatusAccessorRequest ToAccessor(this UpdateLearnedStatusRequest request, Guid userId)
    {
        return new UpdateLearnedStatusAccessorRequest
        {
            UserId = userId,
            CardId = request.CardId,
            IsLearned = request.IsLearned
        };
    }

    /// <summary>
    /// Maps Accessor UpdateLearnedStatusAccessorResponse to frontend UpdateLearnedStatusResponse
    /// </summary>
    public static UpdateLearnedStatusResponse ToFront(this UpdateLearnedStatusAccessorResponse accessorResponse)
    {
        return new UpdateLearnedStatusResponse
        {
            CardId = accessorResponse.CardId,
            IsLearned = accessorResponse.IsLearned
        };
    }

    #endregion
}
