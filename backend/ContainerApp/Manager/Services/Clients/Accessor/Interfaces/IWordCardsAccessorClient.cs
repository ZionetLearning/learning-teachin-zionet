using Manager.Services.Clients.Accessor.Models.WordCards;

namespace Manager.Services.Clients.Accessor.Interfaces;

public interface IWordCardsAccessorClient
{
    Task<GetWordCardsAccessorResponse> GetWordCardsAsync(Guid userId, CancellationToken ct = default);
    Task<CreateWordCardAccessorResponse> CreateWordCardAsync(CreateWordCardAccessorRequest request, CancellationToken ct = default);
    Task<UpdateLearnedStatusAccessorResponse> UpdateLearnedStatusAsync(UpdateLearnedStatusAccessorRequest request, CancellationToken ct = default);
}
