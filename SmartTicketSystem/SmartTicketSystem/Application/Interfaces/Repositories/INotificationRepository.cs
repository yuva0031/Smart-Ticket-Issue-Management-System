using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(long id);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task SaveAsync();
}