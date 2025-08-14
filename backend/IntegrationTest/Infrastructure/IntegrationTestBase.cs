using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<HttpTestFixture>
{
    protected readonly HttpClient Client;

    protected IntegrationTestBase(HttpTestFixture httpFixture)
    {
        Client = httpFixture.Client;
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        return await Client.PostAsJsonAsync(requestUri, value);
    }

    protected async Task<T?> ReadAsJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed with status {response.StatusCode}: {content}");
        }

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Empty response content");

        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
