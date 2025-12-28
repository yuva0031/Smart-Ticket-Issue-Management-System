using SmartTicketSystem.Application.DTOs.Auth;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface IAuthService
{
    Task<string> Register(RegisterUserDto dto);
    Task<AuthResponseDto> Login(LoginDto dto);
}