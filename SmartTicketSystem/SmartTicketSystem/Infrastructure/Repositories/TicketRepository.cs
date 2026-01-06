using Lucene.Net.Store;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    {
        return await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Owner)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId);
    }
    public async Task<IEnumerable<Ticket>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Owner)
            .Include(t => t.AssignedTo)
            .Where(t => t.OwnerId == ownerId)
            .ToListAsync();
    }
    public async Task<IEnumerable<Ticket>> GetByAssignedToAsync(Guid agentId)
    {
        return await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Owner)
                .Include(t => t.AssignedTo)
                .Where(t => t.AssignedToId == agentId)
                .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetUnassignedAsync()
    {
        return await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Owner)
                .Include(t => t.AssignedTo)
                .Where(t => t.AssignedToId == null)
                .ToListAsync();
    }

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

    public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
    {
        return await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Owner)
                .Include(t => t.AssignedTo)
                .ToListAsync(); ;
    }
}