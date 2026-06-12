namespace AuthApi.Application.Security;

public interface IRefreshTokenGenerator
{
    string GenerateToken();
}