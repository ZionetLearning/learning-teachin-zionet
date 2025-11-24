using Manager.Models.UserGameConfiguration.Requests;
using Manager.Models.UserGameConfiguration.Responses;
using Manager.Services.Clients.Accessor.Models.UserGameConfiguration;

namespace Manager.Mapping;

/// <summary>
/// Provides mapping methods between frontend models and accessor models for UserGameConfiguration domain
/// </summary>
public static class UserGameConfigurationMapper
{
    /// <summary>
    /// Maps Accessor response to frontend GetGameConfigResponse
    /// </summary>
    public static GetGameConfigResponse ToFront(this GetGameConfigAccessorResponse accessorResponse)
    {
        return new GetGameConfigResponse
        {
            UserId = accessorResponse.UserId,
            GameName = accessorResponse.GameName,
            Difficulty = accessorResponse.Difficulty,
            Nikud = accessorResponse.Nikud,
            NumberOfSentences = accessorResponse.NumberOfSentences
        };
    }

    /// <summary>
    /// Maps frontend SaveGameConfigRequest to Accessor request
    /// </summary>
    public static SaveGameConfigAccessorRequest ToAccessor(this SaveGameConfigRequest request, Guid userId)
    {
        return new SaveGameConfigAccessorRequest
        {
            UserId = userId,
            GameName = request.GameName,
            Difficulty = request.Difficulty,
            Nikud = request.Nikud,
            NumberOfSentences = request.NumberOfSentences
        };
    }
}