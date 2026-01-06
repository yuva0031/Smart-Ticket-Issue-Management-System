using AutoMapper;

using Moq;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Services.Implementations;

using Xunit;

namespace SmartTicketSystem.Tests.service;

public class TicketPriorityServiceUnitTests
{
    private readonly Mock<ITicketPriorityRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly TicketPriorityService _service;

    public TicketPriorityServiceUnitTests()
    {
        _mockRepo = new Mock<ITicketPriorityRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new TicketPriorityService(_mockRepo.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetPriorityByIdAsync_ValidId_ReturnsEntity()
    {
        // Arrange
        var priorityId = 1;
        var expectedPriority = new TicketPriority { PriorityId = priorityId, PriorityName = "High" };
        _mockRepo.Setup(r => r.GetPriorityByIdAsync(priorityId)).ReturnsAsync(expectedPriority);

        // Act
        var result = await _service.GetPriorityByIdAsync(priorityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("High", result.PriorityName);
        _mockRepo.Verify(r => r.GetPriorityByIdAsync(priorityId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenDatabaseIsEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TicketPriority>());
        _mockMapper.Setup(m => m.Map<IEnumerable<TicketPriorityDto>>(It.IsAny<IEnumerable<TicketPriority>>()))
                   .Returns(new List<TicketPriorityDto>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Empty(result);
        _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateSlaAsync_ExistingPriority_UpdatesHoursAndCallsUpdate()
    {
        // Arrange
        var priorityId = 2;
        var newSla = 24;
        var existingPriority = new TicketPriority { PriorityId = priorityId, SLAHours = 48 };

        _mockRepo.Setup(r => r.GetPriorityByIdAsync(priorityId)).ReturnsAsync(existingPriority);

        // Act
        var result = await _service.UpdateSlaAsync(priorityId, newSla);

        // Assert
        Assert.True(result);
        Assert.Equal(newSla, existingPriority.SLAHours);
        
        _mockRepo.Verify(r => r.UpdateAsync(existingPriority), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public async Task UpdateSlaAsync_ExtremeSlaValues_StillUpdates(int extremeSla)
    {
        // Arrange
        var priority = new TicketPriority { PriorityId = 1, SLAHours = 10 };
        _mockRepo.Setup(r => r.GetPriorityByIdAsync(1)).ReturnsAsync(priority);

        // Act
        var result = await _service.UpdateSlaAsync(1, extremeSla);

        // Assert
        Assert.True(result);
        Assert.Equal(extremeSla, priority.SLAHours);
    }

    [Fact]
    public async Task UpdateSlaAsync_NonExistentPriority_ReturnsFalseAndNeverSaves()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetPriorityByIdAsync(It.IsAny<int>())).ReturnsAsync((TicketPriority?)null);

        // Act
        var result = await _service.UpdateSlaAsync(99, 12);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<TicketPriority>()), Times.Never);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Never);
    }
}