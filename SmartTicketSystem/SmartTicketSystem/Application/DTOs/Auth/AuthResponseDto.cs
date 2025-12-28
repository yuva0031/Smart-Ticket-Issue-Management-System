using SmartTicketSystem.Application.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; }
    public List<string> Roles { get; set; }
    public string Name { get; set; }
}