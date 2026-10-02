using AuthApi.Application.Security;
using AuthApi.Domain.Entities;
using AuthApi.Domain.Repositories;
using AuthApi.Application.Exceptions;

namespace AuthApi.Application.UseCases.RegisterUser;

public class RegisterUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResponse> ExecuteAsync(
        RegisterUserRequest request)
    {
        var emailExists =
            await _userRepository.EmailExistsAsync(request.Email);

        if (emailExists)
        {
            throw new ConflictException("Email already registered.");
        }

        var passwordHash =
            _passwordHasher.HashPassword(request.Password);

        var user = new User(
            request.Name,
            request.Email,
            passwordHash);

        await _userRepository.AddAsync(user);

        return new RegisterUserResponse
        {
            UserId = user.Id,
            Email = user.Email
        };
    }
}