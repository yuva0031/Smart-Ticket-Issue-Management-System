using Microsoft.EntityFrameworkCore;

using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;
    public TicketRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Ticket ticket)
        => await _context.Tickets.AddAsync(ticket);

    public async Task<Ticket?> GetByIdAsync(long ticketId)
        => await _context.Tickets.Include(t => t.Owner).FirstOrDefaultAsync(t => t.TicketId == ticketId);

    public async Task<IEnumerable<Ticket>> GetByOwnerIdAsync(Guid ownerId)
        => await _context.Tickets.Where(t => t.OwnerId == ownerId).ToListAsync();

    public async Task<IEnumerable<Ticket>> GetByAssignedToAsync(Guid agentId)
        => await _context.Tickets.Where(t => t.AssignedToId == agentId).ToListAsync();

    public async Task<IEnumerable<Ticket>> GetUnassignedAsync()
        => await _context.Tickets.Where(t => t.AssignedToId == null).ToListAsync();

    public Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Ticket ticket)
    {
        _context.Tickets.Remove(ticket);
        return Task.CompletedTask;
    }

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}