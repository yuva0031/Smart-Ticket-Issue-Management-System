using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketSystem.Domain.Entities;

public class TicketComment
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long CommentId { get; set; }
    public long TicketId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; }
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Ticket Ticket { get; set; }
    public User User { get; set; }
}