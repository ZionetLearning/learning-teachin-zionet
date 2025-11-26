using Manager.Models.Users;

namespace Manager.Services.Clients.Engine.Models;

public class EmailDraftRequest
{
    public required string Subject { get; set; }
    public required string Purpose { get; set; }
    public required SupportedLanguage Language { get; set; }
}

