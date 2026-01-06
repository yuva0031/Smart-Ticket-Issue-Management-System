using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Domain.Entities;

public class Ticket
{
    public long TicketId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int? CategoryId { get; set; }
    public int PriorityId { get; set; }
    public int StatusId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid? AssignedToId { get; set; }
    public bool IsAutoAssigned { get; set; } = false;
    public int Severity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public TicketCategory Category { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public User Owner { get; set; }
    public User AssignedTo { get; set; }
    public ICollection<TicketHistory> Histories { get; set; }
    public ICollection<TicketComment> Comments { get; set; }
}