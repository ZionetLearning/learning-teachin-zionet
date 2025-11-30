namespace Manager.Services.Clients.Accessor.Interfaces;

public interface IEmailAccessorClient
{
    Task<IReadOnlyList<string>> GetRecipientEmailsByNameAsync(string name, CancellationToken ct = default);
}

