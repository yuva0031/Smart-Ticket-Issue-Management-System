namespace SmartTicketSystem.Application.DTOs;

public class AddTicketCommentDto
{
    public long TicketId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; }
    public bool IsInternal { get; set; }
}