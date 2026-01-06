using AutoMapper;

using SmartTicketSystem.API.Events;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.DTOs.AddTicketCommentDto;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Events;
using SmartTicketSystem.Infrastructure.Utils;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

/// <summary>
/// Service for managing ticket comments, including creation, retrieval, and updates with event notifications.
/// </summary>
public class TicketCommentService : ITicketCommentService
{
    private readonly ITicketCommentRepository _repo;
    private readonly IMapper _mapper;
    private readonly IEventQueue _eventQueue;

    public TicketCommentService(ITicketCommentRepository repo, IMapper mapper, IEventQueue eventQueue)
    {
        _repo = repo;
        _mapper = mapper;
        _eventQueue = eventQueue;
    }

    /// <summary>
    /// Adds a new comment to a ticket and publishes a registration event.
    /// </summary>
    /// <param name="dto">The comment request details.</param>
    /// <param name="userId">The ID of the user posting the comment.</param>
    /// <param name="ticketId">The ID of the ticket the comment belongs to.</param>
    /// <returns>The unique Snowflake ID of the created comment.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the request DTO is null.</exception>
    public async Task<long> AddCommentAsync(AddCommentRequest dto, Guid userId, long ticketId)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var comment = new TicketComment
        {
            CommentId = SnowflakeId.NewId(),
            TicketId = ticketId,
            UserId = userId,
            Message = dto.Message,
            IsInternal = dto.IsInternal,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddCommentAsync(comment);
        await _repo.SaveAsync();

        await _eventQueue.PublishAsync(new TicketCommentAddedEvent(
            comment.CommentId,
            ticketId,
            userId,
            dto.Message,
            dto.IsInternal
        ));

        return comment.CommentId;
    }

    /// <summary>
    /// Retrieves all comments associated with a specific ticket.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A collection of mapped comment responses.</returns>
    public async Task<IEnumerable<CommentResponse>> GetAllByTicketId(long ticketId)
    {
        var comments = await _repo.GetCommentsByTicketAsync(ticketId);
        return _mapper.Map<IEnumerable<CommentResponse>>(comments);
    }

    /// <summary>
    /// Updates an existing comment's message and visibility.
    /// </summary>
    /// <param name="commentId">The ID of the comment to update.</param>
    /// <param name="message">The new comment text.</param>
    /// <param name="isInternal">Visibility status (Internal vs Public).</param>
    /// <returns>True if update was successful; otherwise false.</returns>
    public async Task<bool> UpdateCommentAsync(long commentId, string message, bool isInternal)
    {
        var existing = await _repo.GetByIdAsync(commentId);
        if (existing == null) return false;

        existing.Message = message;
        existing.IsInternal = isInternal;
        existing.CreatedAt = DateTime.UtcNow;

        await _repo.UpdateCommentAsync(existing);
        await _repo.SaveAsync();

        await _eventQueue.PublishAsync(new TicketCommentUpdatedEvent(
            existing.CommentId,
            existing.TicketId,
            existing.UserId,
            existing.Message,
            existing.IsInternal
        ));

        return true;
    }
}