namespace SmartTicketSystem.Application.DTOs;

public class CommentResponse
{
    public long CommentId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; }
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserDisplayName { get; set; }
    public string UserRole { get; set; }
}