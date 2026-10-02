using AuthApi.Application.Exceptions;
using AuthApi.Application.Security;
using AuthApi.Application.UseCases.RegisterUser;
using AuthApi.Domain.Entities;
using AuthApi.Domain.Repositories;
using Moq;

namespace AuthApi.Tests.UseCases.RegisterUser;

public class RegisterUserUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldRegisterUser_WhenEmailIsAvailable()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        userRepositoryMock
            .Setup(x => x.EmailExistsAsync("john@email.com"))
            .ReturnsAsync(false);

        passwordHasherMock
            .Setup(x => x.HashPassword("Password123"))
            .Returns("hashed-password");

        var useCase = new RegisterUserUseCase(
            userRepositoryMock.Object,
            passwordHasherMock.Object);

        var request = new RegisterUserRequest
        {
            Name = "John",
            Email = "john@email.com",
            Password = "Password123"
        };

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.Equal("john@email.com", result.Email);

        userRepositoryMock.Verify(
            x => x.AddAsync(It.Is<User>(user =>
                user.Email == "john@email.com" &&
                user.PasswordHash == "hashed-password")),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowConflictException_WhenEmailAlreadyExists()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        userRepositoryMock
            .Setup(x => x.EmailExistsAsync("john@email.com"))
            .ReturnsAsync(true);

        var useCase = new RegisterUserUseCase(
            userRepositoryMock.Object,
            passwordHasherMock.Object);

        var request = new RegisterUserRequest
        {
            Name = "John",
            Email = "john@email.com",
            Password = "Password123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => useCase.ExecuteAsync(request));

        passwordHasherMock.Verify(
            x => x.HashPassword(It.IsAny<string>()),
            Times.Never);

        userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStorePasswordHash_InsteadOfPlainTextPassword()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        userRepositoryMock
            .Setup(x => x.EmailExistsAsync("john@email.com"))
            .ReturnsAsync(false);

        passwordHasherMock
            .Setup(x => x.HashPassword("Password123"))
            .Returns("hashed-password");

        User? savedUser = null;

        userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => savedUser = user)
            .Returns(Task.CompletedTask);

        var useCase = new RegisterUserUseCase(
            userRepositoryMock.Object,
            passwordHasherMock.Object);

        var request = new RegisterUserRequest
        {
            Name = "John",
            Email = "john@email.com",
            Password = "Password123"
        };

        // Act
        await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(savedUser);
        Assert.Equal("hashed-password", savedUser.PasswordHash);
        Assert.NotEqual("Password123", savedUser.PasswordHash);
    }
}