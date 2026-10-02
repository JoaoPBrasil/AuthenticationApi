using AuthApi.Application.UseCases.Login;
using AuthApi.Application.UseCases.RegisterUser;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthApi.Application.UseCases.RefreshToken;

namespace AuthApi.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _registerUserUseCase;
    private readonly IValidator<RegisterUserRequest> _validator;
    private readonly LoginUseCase _loginUseCase;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly RefreshTokenUseCase _refreshTokenUseCase;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;


    public AuthController(
    RegisterUserUseCase registerUserUseCase,
    LoginUseCase loginUseCase,
    IValidator<RegisterUserRequest> validator,
    IValidator<LoginRequest> loginValidator,
    RefreshTokenUseCase refreshTokenUseCase,
    IValidator<RefreshTokenRequest> refreshTokenValidator)
    {
        _registerUserUseCase = registerUserUseCase;
        _loginUseCase = loginUseCase;
        _validator = validator;
        _loginValidator = loginValidator;
        _refreshTokenUseCase = refreshTokenUseCase;
        _refreshTokenValidator = refreshTokenValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
    RegisterUserRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var response =
            await _registerUserUseCase.ExecuteAsync(request);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var validationResult =
            await _loginValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var response =
            await _loginUseCase.ExecuteAsync(request);

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
    RefreshTokenRequest request)
    {
        var validationResult =
            await _refreshTokenValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var response =
            await _refreshTokenUseCase.ExecuteAsync(request);

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
    RefreshTokenRequest request)
    {
        var validationResult =
            await _refreshTokenValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        await _refreshTokenUseCase
            .LogoutAsync(request.RefreshToken);

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            message = "You are authenticated. ",
            userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
        });
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly()
    {
        return Ok(new
        {
            message = "You have access to the admin area."
        });
    }
}