using AzureFunctionsProject.Common;
using AzureFunctionsProject.Exceptions;
using AzureFunctionsProject.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace AzureFunctionsProject.Manager
{
    public class EngineClient : IEngineClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<EngineClient> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        private const string BaseProcess = "api/" + Routes.EngineProcess;

        public EngineClient(HttpClient httpClient, ILogger<EngineClient> logger)
        {
            _http = httpClient;
            _logger = logger;
        }

        public async Task<ProcessResult> ProcessDataAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Calling Engine endpoint {Path}", BaseProcess);
            try
            {

                var resp = await _http.GetAsync(BaseProcess, ct);
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<ProcessResult>(_jsonOptions, ct)?? throw new InvalidOperationException("Empty result");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to Engine failed");
                throw new EngineClientException("Failed to reach Engine service", ex);
            }
        }
    }
}
