using Microsoft.EntityFrameworkCore;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Repositories;

public class TicketHistoryRepository : ITicketHistoryRepository
{
    private readonly AppDbContext _context;
    public TicketHistoryRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(TicketHistory history)
        => await _context.TicketHistories.AddAsync(history);

    public async Task<IEnumerable<TicketHistory>> GetByTicketIdAsync(long ticketId)
        => await _context.TicketHistories
            .Include(h => h.User)
            .Where(h => h.TicketId == ticketId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}