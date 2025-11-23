using Engine.Models.Emails;

namespace Engine.Services;

public interface IEmailService
{
    Task<EmailDraftResponse> GenerateDraftAsync(EmailDraftRequest request, CancellationToken ct = default);
}

