using AzureFunctionsProject.Models;

namespace AzureFunctionsProject.Services
{
    public interface IDataService
    {
        Task<List<DataDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DataDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task CreateAsync(DataDto entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(DataDto entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }


}
