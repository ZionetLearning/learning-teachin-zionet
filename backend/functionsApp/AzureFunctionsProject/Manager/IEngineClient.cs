using AzureFunctionsProject.Models;

namespace AzureFunctionsProject.Manager
{
    public interface IEngineClient
    {
        Task<ProcessResult> ProcessDataAsync(CancellationToken ct = default);
    }
}
