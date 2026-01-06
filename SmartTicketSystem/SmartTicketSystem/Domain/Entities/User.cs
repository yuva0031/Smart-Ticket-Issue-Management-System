using System.Net.Sockets;

using SmartTicketSystem.Domain.Enums;

namespace SmartTicketSystem.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; }
        public string Email { get; set; }
        public Boolean IsActive { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public UserProfile Profile { get; set; }
        public UserRole UserRole { get; set; }
        public ICollection<AgentProfile> AgentProfiles { get; set; }
        public ICollection<Ticket> OwnedTickets { get; set; }
        public ICollection<Ticket> AssignedTickets { get; set; }
        public ICollection<TicketComment> TicketComments { get; set; }
        public ICollection<TicketHistory> TicketHistories { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}