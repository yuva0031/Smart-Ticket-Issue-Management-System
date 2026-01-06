namespace SmartTicketSystem.API.Events;

public sealed class TicketCommentAddedEvent : IDomainEvent
{
    public long CommentId { get; }
    public long TicketId { get; }
    public Guid UserId { get; }
    public string Message { get; }
    public bool IsInternal { get; }
    public DateTime OccurredAt { get; }

    public TicketCommentAddedEvent(
        long commentId, long ticketId, Guid userId, string message, bool isInternal)
    {
        CommentId = commentId;
        TicketId = ticketId;
        UserId = userId;
        Message = message;
        IsInternal = isInternal;
        OccurredAt = DateTime.UtcNow;
    }
}