using AzureFunctionsProject.Models;

namespace AzureFunctionsProject.Services
{
    public interface IDataService
    {
        Task<List<DataDto>> GetAllAsync();
        Task<DataDto?> GetByIdAsync(Guid id);
        Task CreateAsync(DataDto entity);
        Task UpdateAsync(DataDto entity);
        Task DeleteAsync(Guid id);
    }

}
