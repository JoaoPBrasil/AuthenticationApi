using AuthApi.Application.Exceptions;
using AuthApi.Application.Security;
using AuthApi.Application.UseCases.RefreshToken;
using AuthApi.Domain.Entities;
using AuthApi.Domain.Enums;
using AuthApi.Domain.Repositories;
using Moq;

using RefreshTokenEntity = AuthApi.Domain.Entities.RefreshToken;

namespace AuthApi.Tests.UseCases.RefreshToken;

public class RefreshTokenUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();

        var user = new User(
            "John",
            "john@email.com",
            "hashed-password",
            UserRole.User);

        var storedToken = new RefreshTokenEntity(
            "old-refresh-token",
            DateTime.UtcNow.AddDays(7),
            user.Id);

        jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("new-access-token");

        refreshTokenGeneratorMock
            .Setup(x => x.GenerateToken())
            .Returns("new-refresh-token");

        refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("old-refresh-token"))
            .ReturnsAsync(storedToken);

        var useCase = new RefreshTokenUseCase(
            refreshTokenRepositoryMock.Object,
            jwtTokenGeneratorMock.Object,
            refreshTokenGeneratorMock.Object);

        var request = new RefreshTokenRequest("old-refresh-token");

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.NotEqual("old-refresh-token", result.RefreshToken);

        jwtTokenGeneratorMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Once);

        refreshTokenGeneratorMock.Verify(
            x => x.GenerateToken(),
            Times.Once);

        Assert.True(storedToken.IsRevoked);

        refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(storedToken),
            Times.Once);

        refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.Is<RefreshTokenEntity>(token =>
                token.UserId == user.Id &&
                token.Token == result.RefreshToken)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorizedException_WhenRefreshTokenDoesNotExist()
    {
        // Arrange
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();

        refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("invalid-token"))
            .ReturnsAsync((RefreshTokenEntity?)null);

        var useCase = new RefreshTokenUseCase(
            refreshTokenRepositoryMock.Object,
            jwtTokenGeneratorMock.Object,
            refreshTokenGeneratorMock.Object);

        var request = new RefreshTokenRequest("invalid-token");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => useCase.ExecuteAsync(request));

        jwtTokenGeneratorMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);

        refreshTokenGeneratorMock.Verify(
            x => x.GenerateToken(),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshTokenEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorizedException_WhenRefreshTokenIsExpired()
    {
        // Arrange
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();

        var user = new User(
            "John",
            "john@email.com",
            "hashed-password",
            UserRole.User);

        var expiredToken = new RefreshTokenEntity(
            "expired-token",
            DateTime.UtcNow.AddMinutes(-1),
            user.Id);

        refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("expired-token"))
            .ReturnsAsync(expiredToken);

        var useCase = new RefreshTokenUseCase(
            refreshTokenRepositoryMock.Object,
            jwtTokenGeneratorMock.Object,
            refreshTokenGeneratorMock.Object);

        var request = new RefreshTokenRequest("expired-token");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => useCase.ExecuteAsync(request));

        jwtTokenGeneratorMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);

        refreshTokenGeneratorMock.Verify(
            x => x.GenerateToken(),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<RefreshTokenEntity>()),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshTokenEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorizedException_WhenRefreshTokenIsRevoked()
    {
        // Arrange
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();

        var user = new User(
            "John",
            "john@email.com",
            "hashed-password",
            UserRole.User);

        var revokedToken = new RefreshTokenEntity(
            "revoked-token",
            DateTime.UtcNow.AddDays(7),
            user.Id);

        revokedToken.Revoke();

        refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("revoked-token"))
            .ReturnsAsync(revokedToken);

        var useCase = new RefreshTokenUseCase(
            refreshTokenRepositoryMock.Object,
            jwtTokenGeneratorMock.Object,
            refreshTokenGeneratorMock.Object);

        var request = new RefreshTokenRequest("revoked-token");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => useCase.ExecuteAsync(request));

        jwtTokenGeneratorMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);

        refreshTokenGeneratorMock.Verify(
            x => x.GenerateToken(),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<RefreshTokenEntity>()),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshTokenEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeRefreshToken_WhenTokenIsActive()
    {
        // Arrange
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();

        var user = new User(
            "John",
            "john@email.com",
            "hashed-password",
            UserRole.User);

        var refreshToken = new RefreshTokenEntity(
            "refresh-token",
            DateTime.UtcNow.AddDays(7),
            user.Id);

        refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync("refresh-token"))
            .ReturnsAsync(refreshToken);

        var useCase = new RefreshTokenUseCase(
            refreshTokenRepositoryMock.Object,
            jwtTokenGeneratorMock.Object,
            refreshTokenGeneratorMock.Object);

        // Act
        await useCase.LogoutAsync("refresh-token");

        // Assert
        Assert.True(refreshToken.IsRevoked);

        refreshTokenRepositoryMock.Verify(
            x => x.UpdateAsync(refreshToken),
            Times.Once);
    }
}