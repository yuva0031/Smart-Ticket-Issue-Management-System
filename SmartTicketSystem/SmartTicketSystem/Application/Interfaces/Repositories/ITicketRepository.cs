using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface ITicketRepository
{
    Task<Ticket> GetByIdAsync(long ticketId);
    Task<IEnumerable<Ticket>> GetAllAsync();
    Task<IEnumerable<Ticket>> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<Ticket>> GetByAssignedToIdAsync(Guid assignedToId);
    Task AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);
    Task SaveChangesAsync();
}