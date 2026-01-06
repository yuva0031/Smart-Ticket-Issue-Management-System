using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

/// <summary>
/// Service for managing user notifications, including creation, retrieval, and status updates.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Creates and persists a new notification.
    /// </summary>
    /// <param name="notification">The notification entity to create.</param>
    /// <returns>The created notification with initialized metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when notification is null.</exception>
    public async Task<Notification> CreateAsync(Notification notification)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));

        notification.CreatedAt = DateTime.UtcNow;
        notification.IsRead = false;

        await _repository.AddAsync(notification);
        await _repository.SaveAsync();

        return notification;
    }

    /// <summary>
    /// Retrieves all unread notifications for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of unread notifications.</returns>
    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
    {
        return await _repository.GetUnreadByUserIdAsync(userId);
    }

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    /// <param name="notificationId">The unique identifier of the notification.</param>
    /// <returns>True if the notification was found and updated; otherwise, false.</returns>
    public async Task<bool> MarkAsReadAsync(long notificationId)
    {
        var notification = await _repository.GetByIdAsync(notificationId);
        if (notification == null)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _repository.UpdateAsync(notification);
        await _repository.SaveAsync();

        return true;
    }

    /// <summary>
    /// Marks all unread notifications for a user as read in a single operation.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>True once the operation is completed.</returns>
    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _repository.GetUnreadByUserIdAsync(userId);
        var unreadList = unreadNotifications.ToList();

        if (!unreadList.Any())
            return true;

        var now = DateTime.UtcNow;
        foreach (var notification in unreadList)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _repository.SaveAsync();
        return true;
    }

    /// <summary>
    /// Retrieves a paginated list of all notifications for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="pageNumber">The page index (defaults to 1).</param>
    /// <param name="pageSize">The number of items per page (defaults to 20).</param>
    /// <returns>A paginated collection of notifications.</returns>
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        return await _repository.GetByUserIdAsync(userId, pageNumber, pageSize);
    }
}