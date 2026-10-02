namespace AuthApi.Application.UseCases.RefreshToken;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken);