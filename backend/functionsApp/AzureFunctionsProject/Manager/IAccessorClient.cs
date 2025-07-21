using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureFunctionsProject.Models;

namespace AzureFunctionsProject.Manager
{
    public interface IAccessorClient
    {
        Task<List<DataDto>> GetAllDataAsync();
        Task<DataDto?> GetDataByIdAsync(Guid id);
    }
}
