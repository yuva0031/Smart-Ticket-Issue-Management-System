namespace SmartTicketSystem.Application.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public Boolean IsActive { get; set; }
}