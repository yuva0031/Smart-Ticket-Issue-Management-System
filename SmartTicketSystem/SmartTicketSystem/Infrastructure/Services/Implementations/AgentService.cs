using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

public class AgentService
{
    private readonly IAgentRepository _repo;

    public AgentService(IAgentRepository repo)
    {
        _repo = repo;
    }

    public async Task<string> AddCategorySkill(AddAgentSkillDto dto)
    {
        var profile = await _repo.GetAgentProfileById(dto.AgentProfileId);
        if (profile == null)
            return "Agent profile not found.";

        if (await _repo.SkillExists(dto.AgentProfileId, dto.CategoryId))
            return "Agent already has this skill assigned.";

        await _repo.AddSkillAsync(new AgentCategorySkill
        {
            AgentProfileId = dto.AgentProfileId,
            CategoryId = dto.CategoryId
        });

        await _repo.SaveAsync();
        return "Skill added successfully.";
    }

    public async Task<string> RemoveCategorySkill(RemoveAgentSkillDto dto)
    {
        var skill = await _repo.GetAgentSkill(dto.AgentProfileId, dto.CategoryId);
        if (skill == null)
            return "Skill not found for this agent.";

        await _repo.RemoveSkillAsync(skill);
        await _repo.SaveAsync();
        return "Skill removed successfully.";
    }
}