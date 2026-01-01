using AutoMapper;

using Microsoft.AspNetCore.SignalR;

using SmartTicketSystem.API.Events;
using SmartTicketSystem.API.Hubs;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.DTOs.AddTicketCommentDto;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Events;
using SmartTicketSystem.Infrastructure.Utils;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

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

    public async Task<long> AddCommentAsync(AddCommentRequest dto, Guid userId, long ticketId)
    {
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

        await _eventQueue.PublishAsync(
            new TicketCommentAddedEvent(
                comment.CommentId,
                ticketId,
                userId,
                dto.Message,
                dto.IsInternal,
                comment.CreatedAt
            )
        );
        return comment.CommentId;
    }

    public async Task<IEnumerable<CommentResponse>> GetAllByTicketId(long ticketId)
    {
        var comments = await _repo.GetCommentsByTicketAsync(ticketId);
        return _mapper.Map<IEnumerable<CommentResponse>>(comments);
    }

    public async Task<bool> UpdateCommentAsync(long commentId, string message, bool isInternal)
    {
        var existing = await _repo.GetByIdAsync(commentId);
        if (existing == null) return false;

        existing.Message = message;
        existing.IsInternal = isInternal;
        existing.CreatedAt = DateTime.UtcNow;

        await _repo.UpdateCommentAsync(existing);
        await _repo.SaveAsync();
        await _eventQueue.PublishAsync(
            new TicketCommentUpdatedEvent(
                existing.CommentId,
                existing.TicketId,
                existing.UserId,
                existing.Message,
                existing.IsInternal
            )
        );


        return true;
    }
}