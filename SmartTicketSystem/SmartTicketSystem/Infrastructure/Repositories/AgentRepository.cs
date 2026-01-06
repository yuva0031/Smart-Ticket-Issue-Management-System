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

    public async Task<AgentProfile?> GetByIdAsync(Guid agentProfileId, bool includeSkills = false)
    {
        IQueryable<AgentProfile> query = _context.AgentProfiles;

        if (includeSkills)
        {
            query = query
                .Include(a => a.Skills)
                .ThenInclude(s => s.Category);
        }

        return await query.FirstOrDefaultAsync(a => a.Id == agentProfileId);
    }

    public async Task AddProfileAsync(AgentProfile profile)
    {
        await _context.AgentProfiles.AddAsync(profile);
    }

    public async Task AddSkillAsync(AgentCategorySkill skill)
    {
        await _context.AgentCategorySkills.AddAsync(skill);
    }

    public async Task<AgentProfile?> GetByUserIdAsync(Guid userId, bool includeSkills = false)
    {
        IQueryable<AgentProfile> query = _context.AgentProfiles;

        if (includeSkills)
        {
            query = query
                .Include(a => a.Skills)
                .ThenInclude(s => s.Category);
        }

        return await query.FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<IReadOnlyList<AgentProfile>> GetAllAsync(bool includeSkills = false)
    {
        IQueryable<AgentProfile> query = _context.AgentProfiles;

        if (includeSkills)
        {
            query = query
                .Include(a => a.Skills)
                .ThenInclude(s => s.Category);
        }

        return await query.ToListAsync();
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.TicketCategories.AnyAsync(c => c.CategoryId == categoryId);
    }

    public async Task<IReadOnlyList<TicketCategory>> GetCategoriesByIdsAsync(IEnumerable<int> categoryIds)
    {
        return await _context.TicketCategories
            .Where(c => categoryIds.Contains(c.CategoryId))
            .ToListAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
