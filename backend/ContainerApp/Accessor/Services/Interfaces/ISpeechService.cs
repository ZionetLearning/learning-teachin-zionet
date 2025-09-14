namespace Accessor.Services.Interfaces;

public interface ISpeechService
{
    /// <summary>
    /// Retrieves an Azure Cognitive Services Speech short-lived token.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>JWT token string</returns>
    Task<string> GetSpeechTokenAsync(CancellationToken ct = default);
}