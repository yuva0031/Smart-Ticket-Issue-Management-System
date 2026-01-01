using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Domain.Entities;

public class Notification
{
    public int NotificationId { get; set; }
    public Guid UserId { get; set; }

    public string Message { get; set; }
    public int Channel { get; set; }
    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; }
}