using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface ITicketHistoryRepository
{
    Task AddAsync(TicketHistory history);
    Task<IEnumerable<TicketHistory>> GetByTicketIdAsync(long ticketId);
    Task SaveAsync();
}