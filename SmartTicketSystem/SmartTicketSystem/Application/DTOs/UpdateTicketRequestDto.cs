namespace SmartTicketSystem.Application.DTOs;

public class UpdateTicketRequestDto
{
    public string Description { get; set; }
    public int? CategoryId { get; set; }
    public int? StatusId { get; set; }
    public Guid? AssignedToId { get; set; }
    public int? PriorityId { get; set; }
    public DateTime? DueDate { get; set; }
}