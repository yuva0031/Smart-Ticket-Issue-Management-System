using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs.Auth;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

/// <summary>
/// Controller handling user authentication and registration workflows.
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="registerUserDto">The user registration details.</param>
    /// <returns>A success message or error details.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">If the input data is invalid.</response>
    /// <response code="409">If a user with the same email already exists.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid registration data",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
            });
        }

        var result = await _authService.Register(registerUserDto);

        if (result == "User already exists")
        {
            return Conflict(new
            {
                success = false,
                message = result
            });
        }

        return Ok(new
        {
            success = true,
            message = result
        });
    }

    /// <summary>
    /// Authenticates a user and provides an access token.
    /// </summary>
    /// <param name="loginDto">The login credentials.</param>
    /// <returns>An authentication response containing a JWT token.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="401">Invalid credentials or account pending approval.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var result = await _authService.Login(loginDto);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Specifically handling the "Account pending admin approval" exception from AuthService
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An internal error occurred during login" });
        }
    }
}