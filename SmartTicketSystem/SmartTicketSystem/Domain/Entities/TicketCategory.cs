using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Domain.Entities;

public class TicketCategory
{
    [Key]
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LuceneKeywords { get; set; }

    public int? AutoAssignToRoleId { get; set; }
    public Role AutoAssignToRole { get; set; }

    public ICollection<Ticket> Tickets { get; set; }
}

