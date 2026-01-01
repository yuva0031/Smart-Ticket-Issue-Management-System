using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface ITicketCommentRepository
{
    Task AddCommentAsync(TicketComment comment);
    Task<IEnumerable<TicketComment>> GetCommentsByTicketAsync(long ticketId);
    Task<TicketComment?> GetByIdAsync(long commentId);
    Task UpdateCommentAsync(TicketComment comment);
    Task SaveAsync();
}