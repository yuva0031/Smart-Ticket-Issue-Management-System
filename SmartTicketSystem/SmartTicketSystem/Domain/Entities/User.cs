using SmartTicketSystem.Domain.Enums;

namespace SmartTicketSystem.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public UserProfile Profile { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}