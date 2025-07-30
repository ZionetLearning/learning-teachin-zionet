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
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status {response.StatusCode}: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("Response content is empty or null");
            }

            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to deserialize JSON response. Content: {content}", ex);
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error occurred while reading response", ex);
        }
    }
}