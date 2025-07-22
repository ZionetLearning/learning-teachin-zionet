using AzureFunctionsProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsProject.Manager
{
    public interface IEngineClient
    {
        Task<ProcessResult> ProcessDataAsync(CancellationToken ct = default);
    }
}
