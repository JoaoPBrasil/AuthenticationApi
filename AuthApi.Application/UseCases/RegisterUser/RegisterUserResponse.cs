namespace AuthApi.Application.UseCases.RegisterUser;

public class RegisterUserResponse
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;
}