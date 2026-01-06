using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.DTOs.AddTicketCommentDto;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface ITicketCommentService
{
    Task<long> AddCommentAsync(AddCommentRequest request, Guid userId, long ticketId);
    Task<IEnumerable<CommentResponse>> GetAllByTicketId(long ticketId);
    Task<bool> UpdateCommentAsync(long commentId, string message, bool isInternal);
}