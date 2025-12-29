namespace SmartTicketSystem.Application.DTOs;

public class TicketResponseDto
{
    public long TicketId { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }

    public string Owner { get; set; }
    public string AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}
