using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<TokenResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<TokenResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<TokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}