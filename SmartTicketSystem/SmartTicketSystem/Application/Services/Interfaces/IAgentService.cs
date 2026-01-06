using SmartTicketSystem.Application.DTOs;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface IAgentService
{
    Task<AgentProfileDto?> GetAgentProfileByUserId(Guid userId);
    Task<AgentProfileDto?> GetAgentProfileById(Guid agentProfileId);
    Task<IReadOnlyList<AgentProfileDto>> GetAllAgentProfiles();

    Task<string> AddCategorySkill(AddAgentSkillDto dto);
    Task<string> RemoveCategorySkill(RemoveAgentSkillDto dto);
    Task<string> UpdateAgentSkills(Guid userId, List<int> categoryIds);
}
