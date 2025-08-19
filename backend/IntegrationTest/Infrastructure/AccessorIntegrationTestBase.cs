using System.Net.Http.Json;

namespace IntegrationTests.Infrastructure;

public abstract class AccessorIntegrationTestBase : IClassFixture<AccessorHttpTestFixture>
{
    protected readonly HttpClient Client;

    protected AccessorIntegrationTestBase(AccessorHttpTestFixture httpFixture)
    {
        Client = httpFixture.Client;
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string uri, T value)
        => await Client.PostAsJsonAsync(uri, value);

    protected async Task<T?> ReadAsJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed with status {response.StatusCode}: {content}");
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Empty response content");

        return System.Text.Json.JsonSerializer.Deserialize<T>(
            content,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
