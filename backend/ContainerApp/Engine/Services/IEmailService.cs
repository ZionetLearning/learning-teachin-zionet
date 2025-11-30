using Engine.Models.Emails;

namespace Engine.Services;

public interface IEmailService
{
    Task<EmailDraftResponse> GenerateDraftAsync(string emailPromptContent, CancellationToken ct = default);
    Task SendEmailAsync(SendEmailRequest request, CancellationToken ct = default);
}

