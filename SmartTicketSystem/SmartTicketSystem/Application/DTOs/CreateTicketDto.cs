namespace SmartTicketSystem.Application.DTOs;

public class CreateTicketDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public int PriorityId { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime? DueDate { get; set; }
}
