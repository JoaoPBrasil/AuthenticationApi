using AuthApi.Domain.Entities;

namespace AuthApi.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);

    Task<RefreshToken?> GetByTokenAsync(string token);

    Task UpdateAsync(RefreshToken refreshToken);

    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);

    Task RevokeAllByUserIdAsync(Guid userId);
}