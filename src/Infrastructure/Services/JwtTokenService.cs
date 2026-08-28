using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Application.Configuration;
using Application.DTOs.Auth;
using Application.Interfaces;

using Domain.Entities;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(
        IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public TokenResponse CreateTokens(User user)
    {
        var now = DateTime.UtcNow;

        var accessTokenExpiresOn =
            now.AddMinutes(
                _jwtSettings.AccessTokenExpirationMinutes);

        var refreshTokenExpiresOn =
            now.AddDays(
                _jwtSettings.RefreshTokenExpirationDays);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.UniqueName,
                user.Username),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.Username),

            new(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _jwtSettings.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: accessTokenExpiresOn,
            signingCredentials: credentials);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = GenerateRefreshToken(),
            AccessTokenExpiresOn = accessTokenExpiresOn,
            RefreshTokenExpiresOn = refreshTokenExpiresOn
        };
    }

    public string GenerateRefreshToken()
    {
        var randomBytes =
            RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(
            randomBytes);
    }
}