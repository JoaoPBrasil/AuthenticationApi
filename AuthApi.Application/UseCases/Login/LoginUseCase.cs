using AuthApi.Application.Exceptions;
using AuthApi.Application.Security;
using AuthApi.Domain.Repositories;

namespace AuthApi.Application.UseCases.Login;

public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<LoginResponse> ExecuteAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var passwordValid = _passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        user.RecordLogin();

        await _userRepository.UpdateAsync(user);

        var accessToken = _jwtTokenGenerator.GenerateToken(user);

        var refreshTokenValue = _refreshTokenGenerator.GenerateToken();

        var refreshToken = new Domain.Entities.RefreshToken(
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue
        };
    }
}