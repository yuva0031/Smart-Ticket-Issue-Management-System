using Microsoft.EntityFrameworkCore;

using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
    }

    public async Task<Ticket> GetByIdAsync(long ticketId)
    {
        return await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Owner)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId && !t.IsDeleted);
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync()
    {
        return await _context.Tickets
            .Where(t => !t.IsDeleted)
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Owner)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Tickets
            .Where(t => t.OwnerId == ownerId && !t.IsDeleted)
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByAssignedToIdAsync(Guid assignedToId)
    {
        return await _context.Tickets
            .Where(t => t.AssignedToId == assignedToId && !t.IsDeleted)
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .ToListAsync();
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}