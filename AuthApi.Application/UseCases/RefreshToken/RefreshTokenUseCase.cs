using AuthApi.Application.Security;
using AuthApi.Domain.Repositories;
using AuthApi.Application.Exceptions;

namespace AuthApi.Application.UseCases.RefreshToken;

public class RefreshTokenUseCase
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public RefreshTokenUseCase(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<RefreshTokenResponse> ExecuteAsync(
        RefreshTokenRequest request)
    {
        var storedToken =
            await _refreshTokenRepository
                .GetByTokenAsync(request.RefreshToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            throw new UnauthorizedException(
                "Invalid refresh token.");
        }

        storedToken.Revoke();

        await _refreshTokenRepository
            .UpdateAsync(storedToken);

        var accessToken =
            _jwtTokenGenerator.GenerateToken(
                storedToken.User);

        var newRefreshToken =
            new AuthApi.Domain.Entities.RefreshToken(
                _refreshTokenGenerator.GenerateToken(),
                DateTime.UtcNow.AddDays(7),
                storedToken.UserId);

        await _refreshTokenRepository
            .AddAsync(newRefreshToken);

        return new RefreshTokenResponse(
            accessToken,
            newRefreshToken.Token);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var storedToken =
            await _refreshTokenRepository
                .GetByTokenAsync(refreshToken);

        if (storedToken is null)
            return;

        if (storedToken.IsActive)
        {
            storedToken.Revoke();

            await _refreshTokenRepository
                .UpdateAsync(storedToken);
        }
    }
}