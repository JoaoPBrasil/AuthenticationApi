using AuthApi.Domain.Entities;

namespace AuthApi.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(Guid id);

    Task<bool> EmailExistsAsync(string email);

    Task AddAsync(User user);

    Task UpdateAsync(User user);
}