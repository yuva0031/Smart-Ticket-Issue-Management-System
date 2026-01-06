using Microsoft.EntityFrameworkCore;

using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.UserRole)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> GetSupportAgentsAsync(int? categoryId = null)
    {
        var query = _context.Users.AsQueryable();

        query = from user in query
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.RoleId
                where role.RoleName == "SupportAgent"
                select user;

        if (categoryId.HasValue)
        {
            query = query.Where(u => u.AgentProfiles.Any(ap =>
                ap.Skills.Any(s => s.CategoryId == categoryId.Value)
            ));
        }

        return await query
            .Include(u => u.AgentProfiles)
            .ThenInclude(ap => ap.Skills)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetInactiveUsersAsync()
    {
        return await _context.Users
            .Include(u => u.UserRole)
            .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsActive)
            .ToListAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}