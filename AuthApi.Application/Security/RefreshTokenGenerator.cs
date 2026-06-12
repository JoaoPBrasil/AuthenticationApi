using System.Security.Cryptography;

namespace AuthApi.Application.Security;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string GenerateToken()
    {
        var randomBytes = new byte[64];

        using var rng = RandomNumberGenerator.Create();

        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }
}