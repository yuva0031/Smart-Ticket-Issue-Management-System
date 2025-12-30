using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Domain.Entities;

public class TicketCategory
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}