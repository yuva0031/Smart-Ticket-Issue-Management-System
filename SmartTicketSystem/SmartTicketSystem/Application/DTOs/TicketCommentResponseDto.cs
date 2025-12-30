namespace SmartTicketSystem.Application.DTOs;

public class TicketCommentResponseDto
{
    public int CommentId { get; set; }
    public string Message { get; set; }
    public string CommentedBy { get; set; }
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}