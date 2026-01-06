using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface ITicketRepository
{
    Task AddAsync(Ticket ticket);
    Task<IEnumerable<Ticket>> GetAllTicketsAsync();
    Task<Ticket?> GetByIdAsync(long ticketId);
    Task<IEnumerable<Ticket>> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<Ticket>> GetByAssignedToAsync(Guid agentId);
    Task<IEnumerable<Ticket>> GetUnassignedAsync();
    Task UpdateAsync(Ticket ticket);
    Task DeleteAsync(Ticket ticket);
    Task SaveAsync();
}