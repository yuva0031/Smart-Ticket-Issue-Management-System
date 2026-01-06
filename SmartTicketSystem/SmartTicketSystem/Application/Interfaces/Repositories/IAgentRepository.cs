using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface IAgentRepository
{
    Task<AgentProfile?> GetByIdAsync(Guid agentProfileId, bool includeSkills = false);
    Task<AgentProfile?> GetByUserIdAsync(Guid userId, bool includeSkills = false);
    Task<IReadOnlyList<AgentProfile>> GetAllAsync(bool includeSkills = false);

    Task AddProfileAsync(AgentProfile profile);
    Task AddSkillAsync(AgentCategorySkill skill);

    Task<bool> CategoryExistsAsync(int categoryId);
    Task<IReadOnlyList<TicketCategory>> GetCategoriesByIdsAsync(IEnumerable<int> categoryIds);

    Task SaveAsync();
}
