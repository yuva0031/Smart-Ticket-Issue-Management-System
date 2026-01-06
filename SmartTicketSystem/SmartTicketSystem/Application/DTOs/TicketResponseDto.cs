namespace SmartTicketSystem.Application.DTOs;

public class TicketResponseDto
{
    public long TicketId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int? CategoryId { get; set; }
    public string Category { get; set; }
    public int PriorityId { get; set; }
    public string Priority { get; set; }
    public int StatusId { get; set; }
    public string Status { get; set; }
    public Guid? OwnerId { get; set; }
    public string Owner { get; set; }
    public Guid? AssignedToId { get; set; }
    public string AssignedTo { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}