namespace SmartTicketSystem.Domain.Entities;

public class AgentProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; }
    public ICollection<AgentCategorySkill> Skills { get; set; } = new List<AgentCategorySkill>();
    public int CurrentWorkload { get; set; } = 0;
    public int EscalationLevel { get; set; } = 1;
}