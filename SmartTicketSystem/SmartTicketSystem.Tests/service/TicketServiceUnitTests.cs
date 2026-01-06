using Moq;
using Xunit;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Events;
using SmartTicketSystem.Infrastructure.Services.Implementations;
using SmartTicketSystem.API.Events;

namespace SmartTicketSystem.Tests.service;

public class TicketServiceUnitTests
{
    private readonly Mock<ITicketRepository> _mockRepo;
    private readonly Mock<ITicketPriorityService> _mockPriorityService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IEventQueue> _mockEventQueue;
    private readonly Mock<IHttpContextAccessor> _mockHttpAccessor;
    private readonly TicketService _service;

    public TicketServiceUnitTests()
    {
        _mockRepo = new Mock<ITicketRepository>();
        _mockPriorityService = new Mock<ITicketPriorityService>();
        _mockMapper = new Mock<IMapper>();
        _mockEventQueue = new Mock<IEventQueue>();
        _mockHttpAccessor = new Mock<IHttpContextAccessor>();

        _service = new TicketService(
            _mockRepo.Object,
            _mockPriorityService.Object,
            _mockMapper.Object,
            _mockEventQueue.Object,
            _mockHttpAccessor.Object
        );
    }

    [Fact]
    public async Task CreateAsync_CalculatesSlaAndSetsOwner_Success()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var dto = new CreateTicketDto { PriorityId = 1, Title = "Printer broken" };
        var ticket = new Ticket { CreatedAt = DateTime.UtcNow };

        _mockMapper.Setup(m => m.Map<Ticket>(dto)).Returns(ticket);
        _mockPriorityService.Setup(s => s.GetPriorityByIdAsync(1))
            .ReturnsAsync(new TicketPriority { SLAHours = 24 });

        // Act
        await _service.CreateAsync(dto, ownerId);

        // Assert
        Assert.Equal(ownerId, ticket.OwnerId);
        Assert.Equal(1, ticket.StatusId);
        Assert.NotNull(ticket.DueDate);
        _mockRepo.Verify(r => r.AddAsync(ticket), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_EndUser_CannotChangeStatusOrPriority()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticketId = 100L;
        var existingTicket = new Ticket { 
            TicketId = ticketId, OwnerId = userId, PriorityId = 1, StatusId = 1 
        };
        var updateDto = new UpdateTicketRequestDto { PriorityId = 99, StatusId = 99 };

        SetupMockUserRole("EndUser");
        _mockRepo.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(existingTicket);

        // Act
        await _service.UpdateAsync(ticketId, updateDto, userId);

        // Assert
        Assert.Equal(1, existingTicket.PriorityId);
        Assert.Equal(1, existingTicket.StatusId);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTicketVisibleToUser_Manager_CanSeeAnyTicket()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var randomOwnerId = Guid.NewGuid();
        var ticket = new Ticket { TicketId = 50, OwnerId = randomOwnerId };

        SetupMockUserRole("SupportManager");
        _mockRepo.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(ticket);
        _mockMapper.Setup(m => m.Map<TicketResponseDto>(ticket)).Returns(new TicketResponseDto());

        // Act
        var result = await _service.GetTicketVisibleToUserAsync(50, managerId);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteAsync_NonOwner_ReturnsFalse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var ticket = new Ticket { TicketId = 1, OwnerId = ownerId };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

        // Act
        var result = await _service.DeleteAsync(1, otherUserId);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Ticket>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_UnassignedOwnerTicket_Success()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var ticket = new Ticket { TicketId = 1, OwnerId = ownerId, AssignedToId = null };

        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

        // Act
        var result = await _service.DeleteAsync(1, ownerId);

        // Assert
        Assert.True(result);
        _mockRepo.Verify(r => r.DeleteAsync(ticket), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    private void SetupMockUserRole(string role)
    {
        var claims = new[] { new Claim(ClaimTypes.Role, role) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext { User = principal };
        _mockHttpAccessor.Setup(a => a.HttpContext).Returns(httpContext);
    }
}