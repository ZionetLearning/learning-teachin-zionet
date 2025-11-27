using Manager.Models.Media.Responses;
using Manager.Services.Clients.Accessor.Models.Media;

namespace Manager.Mapping;

/// <summary>
/// Provides mapping methods between frontend models and accessor models for Media domain
/// </summary>
public static class MediaMapper
{
    /// <summary>
    /// Maps Accessor response to frontend GetSpeechTokenResponse
    /// </summary>
    public static GetSpeechTokenResponse ToApiModel(this GetSpeechTokenAccessorResponse accessorResponse)
    {
        return new GetSpeechTokenResponse
        {
            Token = accessorResponse.Token,
            Region = accessorResponse.Region
        };
    }
}
