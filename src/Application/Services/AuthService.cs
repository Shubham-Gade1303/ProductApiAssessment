using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TokenResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();

        var existingUser =
            await _userRepository.GetByUsernameAsync(
                username,
                cancellationToken);

        if (existingUser is not null)
        {
            throw new ArgumentException(
                "Username already exists.");
        }

        if (!Enum.TryParse<UserRole>(
                request.Role,
                true,
                out var role))
        {
            throw new ArgumentException(
                "Invalid user role.");
        }

        var user = new User
        {
            Username = username,
            PasswordHash = _passwordHasher.HashPassword(
                request.Password),
            Role = role,
            CreatedOn = DateTime.UtcNow
        };

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return await CreateTokenResponseAsync(
            user,
            cancellationToken);
    }

    public async Task<TokenResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();

        var user =
            await _userRepository.GetByUsernameAsync(
                username,
                cancellationToken);

        if (user is null)
        {
            throw new ArgumentException(
                "Invalid username or password.");
        }

        var passwordValid =
            _passwordHasher.VerifyPassword(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            throw new ArgumentException(
                "Invalid username or password.");
        }

        return await CreateTokenResponseAsync(
            user,
            cancellationToken);
    }

    public async Task<TokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var existingToken =
            await _refreshTokenRepository.GetByTokenAsync(
                refreshToken,
                cancellationToken);

        if (existingToken is null)
        {
            throw new ArgumentException(
                "Invalid refresh token.");
        }

        if (existingToken.RevokedOn is not null)
        {
            throw new ArgumentException(
                "Refresh token has been revoked.");
        }

        if (existingToken.ExpiresOn <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "Refresh token has expired.");
        }

        existingToken.RevokedOn = DateTime.UtcNow;

        _refreshTokenRepository.Update(existingToken);

        var response = await CreateTokenResponseAsync(
            existingToken.User,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return response;
    }

    public async Task RevokeTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var existingToken =
            await _refreshTokenRepository.GetByTokenAsync(
                refreshToken,
                cancellationToken);

        if (existingToken is null)
        {
            throw new ArgumentException(
                "Invalid refresh token.");
        }

        if (existingToken.RevokedOn is null)
        {
            existingToken.RevokedOn = DateTime.UtcNow;

            _refreshTokenRepository.Update(
                existingToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
    }

    private async Task<TokenResponse> CreateTokenResponseAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var response =
            _jwtTokenService.CreateTokens(user);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = response.RefreshToken,
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = response.RefreshTokenExpiresOn
        };

        await _refreshTokenRepository.AddAsync(
            refreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return response;
    }
}