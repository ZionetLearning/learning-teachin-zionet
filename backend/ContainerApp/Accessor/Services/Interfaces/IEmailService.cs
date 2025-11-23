namespace Accessor.Services.Interfaces;

public interface IEmailService
{
    Task<IReadOnlyList<string>> GetRecipientEmailsByNameAsync(string name, CancellationToken ct = default);
}

