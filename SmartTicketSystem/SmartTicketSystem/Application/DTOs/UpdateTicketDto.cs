namespace SmartTicketSystem.Application.DTOs;

public class UpdateTicketDto
{
    public int StatusId { get; set; }
    public Guid? AssignedToId { get; set; }
    public int? PriorityId { get; set; }
    public DateTime? DueDate { get; set; }
}