using System.Net.Http.Json;
using System.Text.Json;
using AzureFunctionsProject.Common;
using AzureFunctionsProject.Models;

namespace AzureFunctionsProject.Manager
{
    public class AccessorClient : IAccessorClient
    {
        private readonly HttpClient _http;
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

        public AccessorClient(HttpClient httpClient)
            => _http = httpClient;

        public async Task<List<DataDto>> GetAllDataAsync(CancellationToken ct = default)
        {
            var list = await _http.GetFromJsonAsync<List<DataDto>>(
                BaseGetAll,
                _jsonOptions,
                ct
            );
            return list ?? new List<DataDto>();
        }

        public async Task<DataDto?> GetDataByIdAsync(Guid id, CancellationToken ct = default)
        {
            var path = BaseById.Replace("{id}", id.ToString());
            return await _http.GetFromJsonAsync<DataDto>(
                path,
                _jsonOptions,
                ct
            );
        }

        public async Task<DataDto> CreateAsync(DataDto dto, CancellationToken ct = default)
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

        public async Task<DataDto> UpdateAsync(Guid id, DataDto dto, CancellationToken ct = default)
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

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var path = BaseDelete.Replace("{id}", id.ToString());
            var response = await _http.DeleteAsync(path, ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
