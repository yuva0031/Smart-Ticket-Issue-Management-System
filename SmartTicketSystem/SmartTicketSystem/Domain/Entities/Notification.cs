using SmartTicketSystem.Domain.Enums;

namespace SmartTicketSystem.Domain.Entities;

public class Notification
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; }
    public NotificationChannelEnum Channel { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public User User { get; set; }
}