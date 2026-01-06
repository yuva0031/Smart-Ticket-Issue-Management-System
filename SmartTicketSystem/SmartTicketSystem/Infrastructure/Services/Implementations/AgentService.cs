using AutoMapper;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

public class AgentService : IAgentService
{
    private readonly IAgentRepository _repository;
    private readonly IMapper _mapper;

    public AgentService(IAgentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<string> AddCategorySkill(AddAgentSkillDto dto)
    {
        var agentProfile = await _repository.GetByIdAsync(dto.AgentProfileId, includeSkills: true)
            ?? throw new KeyNotFoundException("Agent profile not found");

        if (agentProfile.Skills.Any(s => s.CategoryId == dto.CategoryId))
            return "Agent already has this skill";

        var categoryExists = await _repository.CategoryExistsAsync(dto.CategoryId);
        if (!categoryExists)
            throw new KeyNotFoundException("Category not found");

        agentProfile.Skills.Add(new AgentCategorySkill
        {
            AgentProfileId = agentProfile.Id,
            CategoryId = dto.CategoryId
        });

        await _repository.SaveAsync();
        return "Skill added successfully";
    }

    public async Task<string> RemoveCategorySkill(RemoveAgentSkillDto dto)
    {
        var agentProfile = await _repository.GetByIdAsync(dto.AgentProfileId, includeSkills: true)
            ?? throw new KeyNotFoundException("Agent profile not found");

        var skill = agentProfile.Skills.FirstOrDefault(s => s.CategoryId == dto.CategoryId);
        if (skill == null)
            return "Skill not found";

        agentProfile.Skills.Remove(skill);
        await _repository.SaveAsync();

        return "Skill removed successfully";
    }

    public async Task<AgentProfileDto?> GetAgentProfileByUserId(Guid userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId, includeSkills: true);
        return profile == null ? null : _mapper.Map<AgentProfileDto>(profile);
    }

    public async Task<AgentProfileDto?> GetAgentProfileById(Guid agentProfileId)
    {
        var profile = await _repository.GetByIdAsync(agentProfileId, includeSkills: true);
        return profile == null ? null : _mapper.Map<AgentProfileDto>(profile);
    }

    public async Task<IReadOnlyList<AgentProfileDto>> GetAllAgentProfiles()
    {
        var profiles = await _repository.GetAllAsync(includeSkills: true);
        return _mapper.Map<IReadOnlyList<AgentProfileDto>>(profiles);
    }

    public async Task<string> UpdateAgentSkills(Guid userId, List<int> categoryIds)
    {
        var agentProfile = await _repository.GetByUserIdAsync(userId, includeSkills: true)
            ?? throw new KeyNotFoundException("Agent profile not found");

        if (categoryIds == null || categoryIds.Count == 0)
            throw new InvalidOperationException("At least one category is required");

        var validCategories = await _repository.GetCategoriesByIdsAsync(categoryIds);
        if (validCategories.Count != categoryIds.Distinct().Count())
            throw new InvalidOperationException("One or more categories are invalid");

        agentProfile.Skills.Clear();

        foreach (var categoryId in categoryIds.Distinct())
        {
            agentProfile.Skills.Add(new AgentCategorySkill
            {
                AgentProfileId = agentProfile.Id,
                CategoryId = categoryId
            });
        }

        await _repository.SaveAsync();
        return "Skills updated successfully";
    }
}