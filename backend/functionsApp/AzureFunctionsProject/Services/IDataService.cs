using AzureFunctionsProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
