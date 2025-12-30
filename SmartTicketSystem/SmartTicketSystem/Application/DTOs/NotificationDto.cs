namespace SmartTicketSystem.Application.DTOs;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public string Message { get; set; }
    public string Channel { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}