namespace Engine.Services;

public interface IChatTitleService
{
    Task<string> GenerateTitleAsync(string userMessage, CancellationToken ct = default);
}