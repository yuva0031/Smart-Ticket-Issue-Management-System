using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface IAgentRepository
{
    Task<AgentProfile> GetAgentProfileById(Guid profileId);
    Task<bool> SkillExists(Guid profileId, int categoryId);
    Task<AgentCategorySkill> GetAgentSkill(Guid profileId, int categoryId);
    Task AddProfileAsync(AgentProfile profile);
    Task AddSkillAsync(AgentCategorySkill skill);
    Task RemoveSkillAsync(AgentCategorySkill skill);
    Task SaveAsync();
}