using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface INotificationService
{
    Task<Notification> CreateAsync(Notification notification);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
    Task<bool> MarkAsReadAsync(long notificationId);
    Task<bool> MarkAllAsReadAsync(Guid userId);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20);
}