using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Domain.Entities;

public class TicketStatus
{
    [Key]
    public int StatusId { get; set; }
    public string StatusName { get; set; }
    public ICollection<Ticket> Tickets { get; set; }
}