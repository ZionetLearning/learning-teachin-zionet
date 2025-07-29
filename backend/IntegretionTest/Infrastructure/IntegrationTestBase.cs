using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<HttpTestFixture>
{
    protected readonly HttpTestFixture HttpFixture;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(HttpTestFixture httpFixture)
    {
        HttpFixture = httpFixture;
        Client = httpFixture.Client;
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        return await Client.PostAsJsonAsync(requestUri, value);
    }

    protected async Task<T?> ReadAsJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}