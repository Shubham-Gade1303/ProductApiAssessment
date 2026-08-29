using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock =
            new Mock<IRefreshTokenRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new AuthService(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesUserAndReturnsTokens()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = " admin ",
            Password = "Admin@123",
            Role = "Admin"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(
                "admin",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.HashPassword("Admin@123"))
            .Returns("hashed-password");

        var tokenResponse = new TokenResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            AccessTokenExpiresOn =
                DateTime.UtcNow.AddMinutes(30),
            RefreshTokenExpiresOn =
                DateTime.UtcNow.AddDays(7)
        };

        _jwtTokenServiceMock
            .Setup(x => x.CreateTokens(
                It.IsAny<User>()))
            .Returns(tokenResponse);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);

        _userRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<User>(user =>
                    user.Username == "admin" &&
                    user.PasswordHash == "hashed-password" &&
                    user.Role == UserRole.Admin),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<RefreshToken>(token =>
                    token.Token == "refresh-token"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsArgumentException()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            CreatedOn = DateTime.UtcNow
        };

        var request = new RegisterRequest
        {
            Username = "admin",
            Password = "Admin@123",
            Role = "Admin"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(
                "admin",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RegisterAsync(request));

        Assert.Equal(
            "Username already exists.",
            exception.Message);
    }

    [Fact]
    public async Task RegisterAsync_InvalidRole_ThrowsArgumentException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Password = "Password@123",
            Role = "InvalidRole"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(
                "newuser",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RegisterAsync(request));

        Assert.Equal(
            "Invalid user role.",
            exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = "hashed-password",
            Role = UserRole.Admin,
            CreatedOn = DateTime.UtcNow
        };

        var request = new LoginRequest
        {
            Username = " admin ",
            Password = "Admin@123"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(
                "admin",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(
                "Admin@123",
                "hashed-password"))
            .Returns(true);

        var tokenResponse = new TokenResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            AccessTokenExpiresOn =
                DateTime.UtcNow.AddMinutes(30),
            RefreshTokenExpiresOn =
                DateTime.UtcNow.AddDays(7)
        };

        _jwtTokenServiceMock
            .Setup(x => x.CreateTokens(user))
            .Returns(tokenResponse);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.Equal(
            "access-token",
            result.AccessToken);

        Assert.Equal(
            "refresh-token",
            result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_UserDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "unknown",
            Password = "Password@123"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(
                "unknown",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.LoginAsync(request));

        Assert.Equal(
            "Invalid username or password.",
            exception.Message);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsArgumentException()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = "hashed-password",
            Role = UserRole.Admin,
            CreatedOn = DateTime.UtcNow
        };

        var request = new LoginRequest
        {
            Username = "admin",
            Password = "WrongPassword"
        };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(
                "admin",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(
                "WrongPassword",
                "hashed-password"))
            .Returns(false);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.LoginAsync(request));

        Assert.Equal(
            "Invalid username or password.",
            exception.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_RevokesOldTokenAndCreatesNewToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "admin",
            Role = UserRole.Admin,
            PasswordHash = "hash",
            CreatedOn = DateTime.UtcNow
        };

        var existingToken = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "old-refresh-token",
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(7),
            RevokedOn = null,
            User = user
        };

        var tokenResponse = new TokenResponse
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            AccessTokenExpiresOn =
                DateTime.UtcNow.AddMinutes(30),
            RefreshTokenExpiresOn =
                DateTime.UtcNow.AddDays(7)
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(
                "old-refresh-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _jwtTokenServiceMock
            .Setup(x => x.CreateTokens(user))
            .Returns(tokenResponse);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.RefreshTokenAsync(
            "old-refresh-token");

        // Assert
        Assert.Equal(
            "new-access-token",
            result.AccessToken);

        Assert.Equal(
            "new-refresh-token",
            result.RefreshToken);

        Assert.NotNull(existingToken.RevokedOn);

        _refreshTokenRepositoryMock.Verify(
            x => x.Update(existingToken),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<RefreshToken>(token =>
                    token.Token == "new-refresh-token"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ThrowsArgumentException()
    {
        // Arrange
        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(
                "invalid-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RefreshTokenAsync(
                    "invalid-token"));

        Assert.Equal(
            "Invalid refresh token.",
            exception.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_RevokedToken_ThrowsArgumentException()
    {
        // Arrange
        var token = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "revoked-token",
            CreatedOn = DateTime.UtcNow.AddDays(-1),
            ExpiresOn = DateTime.UtcNow.AddDays(6),
            RevokedOn = DateTime.UtcNow.AddHours(-1),
            User = new User
            {
                Id = 1,
                Username = "admin",
                Role = UserRole.Admin
            }
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(
                "revoked-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.RefreshTokenAsync(
                    "revoked-token"));

        Assert.Equal(
            "Refresh token has been revoked.",
            exception.Message);
    }

    [Fact]
    public async Task RevokeTokenAsync_ValidToken_SetsRevokedOn()
    {
        // Arrange
        var token = new RefreshToken
        {
            Id = 1,
            UserId = 1,
            Token = "refresh-token",
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(7),
            RevokedOn = null
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(
                "refresh-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.RevokeTokenAsync(
            "refresh-token");

        // Assert
        Assert.NotNull(token.RevokedOn);

        _refreshTokenRepositoryMock.Verify(
            x => x.Update(token),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}