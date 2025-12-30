namespace SmartTicketSystem.Application.DTOs;

public class TicketPriorityDto
{
    public int PriorityId { get; set; }
    public string PriorityName { get; set; }
    public int SLAHours { get; set; }
}