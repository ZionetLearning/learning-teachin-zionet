using AzureFunctionsProject.Models;

namespace AzureFunctionsProject.Manager
{
    public interface IAccessorClient
    {
        Task<List<DataDto>> GetAllDataAsync(CancellationToken ct = default);
        Task<DataDto?> GetDataByIdAsync(Guid id, CancellationToken ct = default);
        Task<DataDto> CreateAsync(DataDto dto, CancellationToken ct = default);
        Task<DataDto> UpdateAsync(Guid id, DataDto dto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
