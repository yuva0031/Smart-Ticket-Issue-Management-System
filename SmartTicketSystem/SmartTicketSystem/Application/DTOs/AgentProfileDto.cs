namespace SmartTicketSystem.Application.DTOs;

public class AgentProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int CurrentWorkload { get; set; }
    public List<AgentSkillDto> Skills { get; set; } = new();
}