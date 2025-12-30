using Microsoft.EntityFrameworkCore;

using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Repositories;

public class AgentRepository : IAgentRepository
{
    private readonly AppDbContext _context;

    public AgentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddProfileAsync(AgentProfile profile)
    {
        await _context.AgentProfiles.AddAsync(profile);
    }

    public async Task<AgentProfile> GetAgentProfileById(Guid profileId)
        => await _context.AgentProfiles
            .Include(a => a.Skills)
            .FirstOrDefaultAsync(a => a.Id == profileId);

    public Task<bool> SkillExists(Guid profileId, int categoryId)
        => _context.AgentCategorySkills
            .AnyAsync(s => s.AgentProfileId == profileId && s.CategoryId == categoryId);

    public Task<AgentCategorySkill> GetAgentSkill(Guid profileId, int categoryId)
        => _context.AgentCategorySkills
            .FirstOrDefaultAsync(s => s.AgentProfileId == profileId && s.CategoryId == categoryId);

    public async Task AddSkillAsync(AgentCategorySkill skill)
        => await _context.AgentCategorySkills.AddAsync(skill);

    public async Task RemoveSkillAsync(AgentCategorySkill skill)
        => _context.AgentCategorySkills.Remove(skill);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}