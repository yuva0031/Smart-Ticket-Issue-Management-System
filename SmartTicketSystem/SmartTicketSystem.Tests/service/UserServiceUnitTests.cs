using Moq;
using Xunit;
using AutoMapper;
using SmartTicketSystem.API.Events;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Events;
using SmartTicketSystem.Infrastructure.Services.Implementations;

namespace SmartTicketSystem.Tests.service;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IEventQueue> _mockEventQueue;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockEventQueue = new Mock<IEventQueue>();

        _service = new UserService(
            _mockRepo.Object,
            _mockMapper.Object,
            _mockEventQueue.Object
        );
    }

    [Fact]
    public async Task ApproveUserAsync_ValidInactiveUser_ActivatesAndPublishesEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = false };

        _mockRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        await _service.ApproveUserAsync(userId, adminId);

        // Assert
        Assert.True(user.IsActive);
        _mockRepo.Verify(r => r.UpdateAsync(user), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        
        _mockEventQueue.Verify(e => e.PublishAsync(It.Is<UserApprovedEvent>(ev => 
            ev.UserId == userId && ev.ApprovedBy == adminId)), Times.Once);
    }

    [Fact]
    public async Task ApproveUserAsync_AlreadyActiveUser_DoesNothing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };
        _mockRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        await _service.ApproveUserAsync(userId, Guid.NewGuid());

        // Assert
        _mockRepo.Verify(r => r.SaveAsync(), Times.Never);
        _mockEventQueue.Verify(e => e.PublishAsync(It.IsAny<UserApprovedEvent>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPendingUsersAsync_ReturnsOnlyInactiveUsers()
    {
        // Arrange
        var inactiveUsers = new List<User> { new User { IsActive = false } };
        _mockRepo.Setup(r => r.GetInactiveUsersAsync()).ReturnsAsync(inactiveUsers);
        _mockMapper.Setup(m => m.Map<IEnumerable<UserDto>>(inactiveUsers))
            .Returns(new List<UserDto> { new UserDto { FullName = "Pending User" } });

        // Act
        var result = await _service.GetPendingUsersAsync();

        // Assert
        Assert.Single(result);
        _mockRepo.Verify(r => r.GetInactiveUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersWithRolesAsync_CallsRepositoryAndMapper()
    {
        // Arrange
        var users = new List<User> { new User(), new User() };
        _mockRepo.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);

        // Act
        await _service.GetAllUsersWithRolesAsync();

        // Assert
        _mockRepo.Verify(r => r.GetAllUsersAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<IEnumerable<UserDto>>(users), Times.Once);
    }
}