using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Domain.Entities;

public class TicketHistory
{
    public int HistoryId { get; set; }
    public long TicketId { get; set; }
    public Guid ModifiedBy { get; set; }
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public Ticket Ticket { get; set; }
    public User User { get; set; }
}