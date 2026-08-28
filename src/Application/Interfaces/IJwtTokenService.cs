using Application.DTOs.Auth;
using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    TokenResponse CreateTokens(User user);

    string GenerateRefreshToken();
}