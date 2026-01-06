namespace SmartTicketSystem.API.Events;

public sealed class TicketCommentUpdatedEvent : IDomainEvent
{
    public long CommentId { get; }
    public long TicketId { get; }
    public Guid UserId { get; }
    public string Message { get; }
    public bool IsInternal { get; }
    public DateTime UpdatedAt { get; }

    public DateTime OccurredAt { get; }

    public TicketCommentUpdatedEvent(long commentId, long ticketId, Guid userId, string message, bool isInternal)
    {
        CommentId = commentId;
        TicketId = ticketId;
        UserId = userId;
        Message = message;
        IsInternal = isInternal;
        UpdatedAt = DateTime.UtcNow;
    }
}