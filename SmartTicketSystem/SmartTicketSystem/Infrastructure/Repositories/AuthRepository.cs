using Microsoft.EntityFrameworkCore;

using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;
    public AuthRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByEmailWithRole(string email)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> GetByEmail(string email) =>
        await _context.Users.Include(x => x.Profile).FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task AddAgentProfile(AgentProfile profile)
    {
        await _context.AgentProfiles.AddAsync(profile);
    }

    public async Task AddUser(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task Save() => await _context.SaveChangesAsync();

    public async Task<List<Guid>> GetUserIdsByRoles(int[] roleIds)
    {
        return await _context.Users
            .Where(u => u.UserRole != null && roleIds.Contains(u.UserRole.RoleId))
            .Select(u => u.Id)
            .ToListAsync();
    }


}