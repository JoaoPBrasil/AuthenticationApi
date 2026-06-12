using AuthApi.Domain.Entities;

namespace AuthApi.Application.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}