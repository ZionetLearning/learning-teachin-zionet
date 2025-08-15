using Manager.Models.Auth;

namespace Manager.Services;

public interface IAuthService
{
    Task<(string accessToken, string refreshToken)> LoginAsync(LoginRequest loginRequest, HttpRequest httpRequest);

    Task<(string accessToken, string refreshToken)> RefreshTokensAsync(HttpRequest request);

    Task LogoutAsync(HttpRequest request);

}