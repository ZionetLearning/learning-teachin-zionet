using Manager.Models.Auth;

namespace Manager.Services;

public interface IAuthService
{
    Task<(string accessToken, string refreshToken)> LoginAsync(LoginRequest loginRequest, HttpRequest httpRequest, CancellationToken cancellationToken);

    Task<(string accessToken, string refreshToken)> RefreshTokensAsync(HttpRequest request, CancellationToken cancellationToken);

    Task LogoutAsync(HttpRequest request, CancellationToken cancellationToken);

}