using Engine.Models.Emails;

namespace Engine.Services;

public interface IEmailService
{
    Task<EmailDraftResponse> GenerateDraftAsync(string emailPromptContent, CancellationToken ct = default);
}

