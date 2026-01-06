namespace SmartTicketSystem.Application.DTOs;

public class UserProfileDto
{
    public Guid UserProfileId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}