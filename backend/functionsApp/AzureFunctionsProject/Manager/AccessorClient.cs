using AzureFunctionsProject.Common;
using AzureFunctionsProject.Exceptions;
using AzureFunctionsProject.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace AzureFunctionsProject.Manager
{
    public class AccessorClient : IAccessorClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<AccessorClient> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Prepend the Functions host's default `/api/` prefix to your route constants
        private const string BaseGetAll = "api/" + Routes.AccessorGetAll;     // "api/accessor/data"
        private const string BaseById = "api/" + Routes.AccessorGetById;    // "api/accessor/data/{id}"
        private const string BaseCreate = "api/" + Routes.AccessorCreate;     // "api/accessor/data"
        private const string BaseUpdate = "api/" + Routes.AccessorUpdate;     // "api/accessor/data/{id}"
        private const string BaseDelete = "api/" + Routes.AccessorDelete;     // "api/accessor/data/{id}"

        public AccessorClient(HttpClient httpClient, ILogger<AccessorClient> logger)
        {
            _http = httpClient;
            _logger = logger;
        }

        public async Task<List<DataDto>> GetAllDataAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Accessor: GET all data");
            try
            {
                var list = await _http.GetFromJsonAsync<List<DataDto>>(BaseGetAll,_jsonOptions,ct);
                return list ?? new List<DataDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Accessor: GET all data failed");
                throw new AccessorClientException(
                    "Failed to retrieve all Data from the Accessor service", ex);
            }
        }

        public async Task<DataDto?> GetDataByIdAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Accessor: GET data/{Id}", id);
            try
            {
                var path = BaseById.Replace("{id}", id.ToString());
            return await _http.GetFromJsonAsync<DataDto>(
                path,
                _jsonOptions,
                ct
            );
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Accessor: GET data/{Id} failed", id);
                throw new AccessorClientException(
                    "Failed to retrieve all Data from the Accessor service", ex);
            }
        }

        public async Task<DataDto> CreateAsync(DataDto dto, CancellationToken ct = default)
        {
            _logger.LogInformation("Accessor: POST data (Id={Id})", dto.Id);
            try
            {
                var response = await _http.PostAsJsonAsync(
                BaseCreate,
                dto,
                _jsonOptions,
                ct
            );
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<DataDto>(_jsonOptions, ct)
                       ?? throw new InvalidOperationException("Empty response body.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Accessor: POST data failed");
                throw new AccessorClientException(
                    "Failed to retrieve all Data from the Accessor service", ex);
            }
        }

        public async Task<DataDto> UpdateAsync(Guid id, DataDto dto, CancellationToken ct = default)
        {
            _logger.LogInformation("Accessor: PUT data/{Id}@v{Version}", id, dto.Version);

            try
            {
                var path = BaseUpdate.Replace("{id}", id.ToString());

            var request = new HttpRequestMessage(HttpMethod.Put, path)
            {
                Content = JsonContent.Create(dto, options: _jsonOptions)
            };

            var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DataDto>(_jsonOptions, ct)
                   ?? throw new InvalidOperationException("Empty response body.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Accessor: PUT data/{Id} failed", id);
                throw new AccessorClientException(
                    "Failed to retrieve all Data from the Accessor service", ex);
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Accessor: DELETE data/{Id}", id);
            try
            {

                var path = BaseDelete.Replace("{id}", id.ToString());
            var response = await _http.DeleteAsync(path, ct);
            response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Accessor: DELETE data/{Id} failed", id);
                throw new AccessorClientException(
                    "Failed to retrieve all Data from the Accessor service", ex);
            }
        }
    }
}
