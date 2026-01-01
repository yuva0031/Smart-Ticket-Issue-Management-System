using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Domain.Entities;

public class TicketPriority
{
    public int PriorityId { get; set; }
    public string PriorityName { get; set; }
    public int SLAHours { get; set; }
    public ICollection<Ticket> Tickets { get; set; }
}