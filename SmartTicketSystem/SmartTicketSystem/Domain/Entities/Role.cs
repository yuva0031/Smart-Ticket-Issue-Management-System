using SmartTicketSystem.Domain.Enums;

namespace SmartTicketSystem.Domain.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}