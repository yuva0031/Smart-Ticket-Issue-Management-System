using AutoMapper;

using Moq;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Services.Implementations;

using Xunit;

namespace SmartTicketSystem.Tests.service;

public class TicketHistoryServiceUnitTests
{
    private readonly Mock<ITicketHistoryRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly TicketHistoryService _service;

    public TicketHistoryServiceUnitTests()
    {
        _mockRepo = new Mock<ITicketHistoryRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new TicketHistoryService(_mockRepo.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task LogChangeAsync_ValidDto_SetsTimestampAndSaves()
    {
        // Arrange
        var dto = new CreateTicketHistoryDto
        {
            TicketId = 1L,
            FieldName = "Priority",
            OldValue = "Low",
            NewValue = "High",
            ModifiedBy = Guid.NewGuid()
        };
        var log = new TicketHistory(); 

        _mockMapper.Setup(m => m.Map<TicketHistory>(dto)).Returns(log);

        // Act
        await _service.LogChangeAsync(dto);

        // Assert
        Assert.NotEqual(default, log.ChangedAt);
        Assert.True((DateTime.UtcNow - log.ChangedAt).TotalSeconds < 2);
        
        _mockRepo.Verify(r => r.AddAsync(log), Times.Once);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task LogChangeAsync_NullDto_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.LogChangeAsync(null!));
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<TicketHistory>()), Times.Never);
    }

    [Fact]
    public async Task GetHistoryAsync_TicketWithNoHistory_ReturnsEmptyList()
    {
        // Arrange
        long ticketId = 999L;
        _mockRepo.Setup(r => r.GetByTicketIdAsync(ticketId))
                 .ReturnsAsync(new List<TicketHistory>());
        
        _mockMapper.Setup(m => m.Map<IEnumerable<TicketHistoryResponseDto>>(It.IsAny<IEnumerable<TicketHistory>>()))
                   .Returns(new List<TicketHistoryResponseDto>());

        // Act
        var result = await _service.GetHistoryAsync(ticketId);

        // Assert
        Assert.Empty(result);
        _mockRepo.Verify(r => r.GetByTicketIdAsync(ticketId), Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_ValidTicket_CallsMapperWithCorrectData()
    {
        // Arrange
        long ticketId = 1L;
        var rawLogs = new List<TicketHistory> 
        { 
            new TicketHistory { TicketId = ticketId, FieldName = "Status" } 
        };
        
        _mockRepo.Setup(r => r.GetByTicketIdAsync(ticketId)).ReturnsAsync(rawLogs);

        // Act
        await _service.GetHistoryAsync(ticketId);

        // Assert
        _mockMapper.Verify(m => m.Map<IEnumerable<TicketHistoryResponseDto>>(rawLogs), Times.Once);
    }
}