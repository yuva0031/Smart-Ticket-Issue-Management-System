using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs.Auth;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

[Route("/api/controller")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("/register")]
    public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
    {
        var result = await _authService.Register(registerUserDto);
        return Ok(result);
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var result = await _authService.Login(loginDto);
        if (result == null) return Unauthorized("Invalid credentials");
        return Ok(result);
    }


}