using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AzureFunctionsProject.Models;

namespace AzureFunctionsProject.Manager
{
    public class AccessorClient : IAccessorClient
    {
        private readonly HttpClient _http;
        // this matches the route on your Accessor function
        private const string BasePath = "api/accessor/data";

        public AccessorClient(HttpClient httpClient)
            => _http = httpClient;

        public Task<List<DataDto>> GetAllDataAsync()
            // if the function returns null, give back an empty list
            => _http.GetFromJsonAsync<List<DataDto>>(BasePath)
               ?? Task.FromResult(new List<DataDto>());

        public Task<DataDto?> GetDataByIdAsync(Guid id)
            => _http.GetFromJsonAsync<DataDto>($"{BasePath}/{id}");
    }
}
