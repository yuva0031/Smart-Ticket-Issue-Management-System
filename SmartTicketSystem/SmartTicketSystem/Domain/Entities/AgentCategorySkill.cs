namespace SmartTicketSystem.Domain.Entities;

public class AgentCategorySkill
{
    public int Id { get; set; }

    public Guid AgentProfileId { get; set; }
    public AgentProfile AgentProfile { get; set; }
    public int CategoryId { get; set; }
    public TicketCategory Category { get; set; }
}