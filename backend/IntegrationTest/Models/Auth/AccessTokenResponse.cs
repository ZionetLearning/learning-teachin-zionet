
using System.Text.Json.Serialization;

namespace IntegrationTests.Models.Auth;
public class AccessTokenResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = default!;
}

