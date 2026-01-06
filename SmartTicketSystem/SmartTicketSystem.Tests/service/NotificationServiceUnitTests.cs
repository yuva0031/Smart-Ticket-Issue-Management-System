using Moq;

using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Services.Implementations;

using Xunit;

namespace SmartTicketSystem.Tests.service;

public class NotificationServiceUnitTests
{
    private readonly Mock<INotificationRepository> _mockRepo;
    private readonly NotificationService _service;

    public NotificationServiceUnitTests()
    {
        _mockRepo = new Mock<INotificationRepository>();
        _service = new NotificationService(_mockRepo.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidNotification_SetsInitialPropertiesAndSaves()
    {
        // Arrange
        var notification = new Notification { Message = "New Ticket Created", UserId = Guid.NewGuid() };

        // Act
        var result = await _service.CreateAsync(notification);

        // Assert
        Assert.False(result.IsRead);
        Assert.NotNull(result.CreatedAt);
        Assert.True((DateTime.UtcNow - result.CreatedAt).TotalSeconds < 2);
        
        _mockRepo.Verify(r => r.AddAsync(notification), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NullNotification_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
    }

    [Fact]
    public async Task MarkAsReadAsync_ExistingNotification_UpdatesStatusAndReturnsTrue()
    {
        // Arrange
        long notificationId = 123;
        var notification = new Notification { Id = notificationId, IsRead = false };
        _mockRepo.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);

        // Act
        var result = await _service.MarkAsReadAsync(notificationId);

        // Assert
        Assert.True(result);
        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
        _mockRepo.Verify(r => r.UpdateAsync(notification), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_NoUnreadNotifications_ReturnsTrueWithoutSaving()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetUnreadByUserIdAsync(userId))
                 .ReturnsAsync(new List<Notification>()); 

        // Act
        var result = await _service.MarkAllAsReadAsync(userId);

        // Assert
        Assert.True(result);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task GetByUserIdAsync_UsesDefaultPagination_WhenParametersNotProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        await _service.GetByUserIdAsync(userId); 

        // Assert
        _mockRepo.Verify(r => r.GetByUserIdAsync(userId, 1, 20), Times.Once);
    }

    [Fact]
    public async Task GetUnreadByUserIdAsync_ReturnsOnlyUnreadItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var unreadItems = new List<Notification> { new Notification { IsRead = false } };
        _mockRepo.Setup(r => r.GetUnreadByUserIdAsync(userId)).ReturnsAsync(unreadItems);

        // Act
        var result = await _service.GetUnreadByUserIdAsync(userId);

        // Assert
        Assert.Single(result);
        Assert.All(result, n => Assert.False(n.IsRead));
    }
}