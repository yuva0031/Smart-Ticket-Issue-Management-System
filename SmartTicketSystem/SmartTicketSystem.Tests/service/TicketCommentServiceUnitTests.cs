using Moq;
using AutoMapper;
using SmartTicketSystem.API.Events;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.DTOs.AddTicketCommentDto;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Events;
using SmartTicketSystem.Infrastructure.Services.Implementations;
using Xunit;

namespace SmartTicketSystem.Tests.service;

public class TicketCommentServiceUnitTests
{
    private readonly Mock<ITicketCommentRepository> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IEventQueue> _mockEventQueue;
    private readonly TicketCommentService _service;

    public TicketCommentServiceUnitTests()
    {
        _mockRepo = new Mock<ITicketCommentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockEventQueue = new Mock<IEventQueue>();

        _service = new TicketCommentService(
            _mockRepo.Object,
            _mockMapper.Object,
            _mockEventQueue.Object
        );
    }

    [Fact]
    public async Task AddCommentAsync_ValidRequest_PersistsWithSnowflakeId()
    {
        // Arrange
        var dto = new AddCommentRequest { Message = "Valid comment", IsInternal = true };
        var userId = Guid.NewGuid();
        var ticketId = 555L;

        // Act
        var resultId = await _service.AddCommentAsync(dto, userId, ticketId);

        // Assert
        Assert.NotEqual(0, resultId);
        _mockRepo.Verify(r => r.AddCommentAsync(It.Is<TicketComment>(c =>
            c.CommentId == resultId &&
            c.TicketId == ticketId &&
            c.Message == dto.Message)), Times.Once);

        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);

        _mockEventQueue.Verify(e => e.PublishAsync(It.Is<TicketCommentAddedEvent>(ev =>
            ev.CommentId == resultId &&
            ev.TicketId == ticketId &&
            ev.IsInternal == true)), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_NullDto_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.AddCommentAsync(null!, Guid.NewGuid(), 1L));
    }

    [Fact]
    public async Task UpdateCommentAsync_ExistingComment_UpdatesTimestamps()
    {
        // Arrange
        var commentId = 123L;
        var oldDate = DateTime.UtcNow.AddDays(-1);
        var existing = new TicketComment
        {
            CommentId = commentId,
            Message = "Old Message",
            CreatedAt = oldDate
        };

        _mockRepo.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(existing);

        // Act
        await _service.UpdateCommentAsync(commentId, "Updated Message", false);

        // Assert
        Assert.Equal("Updated Message", existing.Message);
        // Verify CreatedAt was updated to current time
        Assert.True(existing.CreatedAt > oldDate);
        _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        _mockEventQueue.Verify(e => e.PublishAsync(It.IsAny<TicketCommentUpdatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task GetAllByTicketId_WhenNoCommentsFound_ReturnsEmptyCollection()
    {
        // Arrange
        var ticketId = 999L;
        var emptyComments = new List<TicketComment>();
        var emptyResponses = new List<CommentResponse>();

        _mockRepo.Setup(r => r.GetCommentsByTicketAsync(ticketId))
                 .ReturnsAsync(emptyComments);

        // FIX: Explicitly setup the mapper to handle the exact IEnumerable signature
        _mockMapper.Setup(m => m.Map<IEnumerable<CommentResponse>>(It.IsAny<IEnumerable<TicketComment>>()))
                   .Returns(emptyResponses.AsEnumerable()); // This ensures type compatibility

        // Act
        var result = await _service.GetAllByTicketId(ticketId);

        // Assert
        Assert.Empty(result);
        _mockRepo.Verify(r => r.GetCommentsByTicketAsync(ticketId), Times.Once);
    }
}