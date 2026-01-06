using Microsoft.EntityFrameworkCore;

using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Repositories;

public class TicketPriorityRepository : ITicketPriorityRepository
{
    private readonly AppDbContext _context;

    public TicketPriorityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TicketPriority>> GetAllAsync() =>
        await _context.TicketPriorities.AsNoTracking().ToListAsync();

    public async Task<TicketPriority?> GetPriorityByIdAsync(int priorityId) =>
        await _context.TicketPriorities.FirstOrDefaultAsync(p => p.PriorityId == priorityId);

    public async Task UpdateAsync(TicketPriority priority)
    {
        _context.TicketPriorities.Update(priority);
        await Task.CompletedTask;
    }

    public async Task SaveAsync() =>
        await _context.SaveChangesAsync();
}