using AzureFunctionsProject.Common;
using AzureFunctionsProject.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace AzureFunctionsProject.Manager
{
    public class EngineClient : IEngineClient
    {
        private readonly HttpClient _http;
        private const string BaseProcess = "api/" + Routes.EngineProcess;

        public EngineClient(HttpClient httpClient) => _http = httpClient;

        public async Task<ProcessResult> ProcessDataAsync(CancellationToken ct = default)
        {
            var resp = await _http.GetAsync(BaseProcess, ct);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ProcessResult>(
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct)
                   ?? throw new InvalidOperationException("Empty result");
        }
    }
}
