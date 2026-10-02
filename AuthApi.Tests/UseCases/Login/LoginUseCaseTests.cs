using AuthApi.Application.Exceptions;
using AuthApi.Application.Security;
using AuthApi.Application.UseCases.Login;
using AuthApi.Domain.Entities;
using AuthApi.Domain.Enums;
using AuthApi.Domain.Repositories;
using Moq;

using RefreshTokenEntity = AuthApi.Domain.Entities.RefreshToken;

namespace AuthApi.Tests.UseCases.Login;

public class LoginUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var user = new User(
            "John",
            "john@email.com",
            "hashed-password",
            UserRole.User);

        userRepositoryMock
            .Setup(x => x.GetByEmailAsync("john@email.com"))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(x => x.VerifyPassword(
                "Password123",
                "hashed-password"))
            .Returns(true);

        jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(user))
            .Returns("access-token");

        refreshTokenGeneratorMock
            .Setup(x => x.GenerateToken())
            .Returns("refresh-token");

        var useCase = new LoginUseCase(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenGeneratorMock.Object,
            refreshTokenGeneratorMock.Object,
            refreshTokenRepositoryMock.Object);

        var request = new LoginRequest
        {
            Email = "john@email.com",
            Password = "Password123"
        };

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);

        userRepositoryMock.Verify(
            x => x.UpdateAsync(user),
            Times.Once);

        passwordHasherMock.Verify(
            x => x.VerifyPassword(
                "Password123",
                "hashed-password"),
            Times.Once);

        refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.Is<RefreshTokenEntity>(token =>
                token.Token == "refresh-token" &&
                token.UserId == user.Id)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorizedException_WhenUserDoesNotExist()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        userRepositoryMock
            .Setup(x => x.GetByEmailAsync("unknown@email.com"))
            .ReturnsAsync((User?)null);

        var useCase = new LoginUseCase(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenGeneratorMock.Object,
            refreshTokenGeneratorMock.Object,
            refreshTokenRepositoryMock.Object);

        var request = new LoginRequest
        {
            Email = "unknown@email.com",
            Password = "Password123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => useCase.ExecuteAsync(request));

        passwordHasherMock.Verify(
            x => x.VerifyPassword(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        jwtTokenGeneratorMock.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);

        refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshTokenEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorizedException_WhenPasswordIsInvalid()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var user = new User(
            "John",
            "john@email.com",
            "hashed-password",
            UserRole.User);

        userRepositoryMock
            .Setup(x => x.GetByEmailAsync("john@email.com"))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(x => x.VerifyPassword(
                "WrongPassword",
                "hashed-password"))
            .Returns(false);

        var useCase = new LoginUseCase(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenGeneratorMock.Object,
            refreshTokenGeneratorMock.Object,
            refreshTokenRepositoryMock.Object);

        var request = new LoginRequest
        {
            Email = "john@email.com",
            Password = "WrongPassword"
        };

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
    public async Task ExecuteAsync_ShouldRecordLogin_WhenCredentialsAreValid()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();
        var refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        var user = new User(
            "John",
            "john@email.com",
            "hashed-password",
            UserRole.User);

        userRepositoryMock
            .Setup(x => x.GetByEmailAsync("john@email.com"))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(x => x.VerifyPassword(
                "Password123",
                "hashed-password"))
            .Returns(true);

        jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(user))
            .Returns("access-token");

        refreshTokenGeneratorMock
            .Setup(x => x.GenerateToken())
            .Returns("refresh-token");

        DateTime? lastLoginBefore = user.LastLoginAt;

        var useCase = new LoginUseCase(
            userRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenGeneratorMock.Object,
            refreshTokenGeneratorMock.Object,
            refreshTokenRepositoryMock.Object);

        var request = new LoginRequest
        {
            Email = "john@email.com",
            Password = "Password123"
        };

        // Act
        await useCase.ExecuteAsync(request);

        // Assert
        Assert.Null(lastLoginBefore);
        Assert.NotNull(user.LastLoginAt);

        userRepositoryMock.Verify(
            x => x.UpdateAsync(user),
            Times.Once);
    }
}