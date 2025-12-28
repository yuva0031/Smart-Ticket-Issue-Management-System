using Microsoft.EntityFrameworkCore;

using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByEmailWithRoles(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> GetByEmail(string email) =>
        await _context.Users.Include(x => x.Profile).FirstOrDefaultAsync(u => u.Email == email);

    public async Task AddUser(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task Save() => await _context.SaveChangesAsync();
}