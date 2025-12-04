using Engine.Models.Users;

namespace Engine.Models.Emails;

public class EmailDraftRequest
{
    public required Guid UserId { get; set; }
    public required string Subject { get; set; }
    public required string Purpose { get; set; }
    public required SupportedLanguage Language { get; set; }
}

