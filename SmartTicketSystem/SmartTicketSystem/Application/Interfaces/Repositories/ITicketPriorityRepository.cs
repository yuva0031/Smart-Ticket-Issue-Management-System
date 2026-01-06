using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface ITicketPriorityRepository
{
    Task<IEnumerable<TicketPriority>> GetAllAsync();
    Task<TicketPriority?> GetPriorityByIdAsync(int priorityId);
    Task UpdateAsync(TicketPriority ticketPriority);
    Task SaveAsync();
}