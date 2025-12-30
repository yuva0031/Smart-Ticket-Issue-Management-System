namespace SmartTicketSystem.Application.DTOs.Auth;

public class RegisterUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<int> RoleIds { get; set; }
    public List<int>? CategorySkillIds { get; set; }
    public string PhoneNumber { get; set; }
    public string Department { get; set; }
    public string Address { get; set; }
}