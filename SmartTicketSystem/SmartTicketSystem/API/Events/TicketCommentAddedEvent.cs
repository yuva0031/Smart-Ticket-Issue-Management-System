namespace SmartTicketSystem.API.Events;

public sealed class TicketCommentAddedEvent : IDomainEvent
{
    public long CommentId { get; }
    public long TicketId { get; }
    public Guid UserId { get; }
    public string Message { get; }
    public bool IsInternal { get; }
    public DateTime CreatedAt { get; }

    public TicketCommentAddedEvent(
        long commentId, long ticketId, Guid userId, string message, bool isInternal, DateTime createdAt)
    {
        CommentId = commentId;
        TicketId = ticketId;
        UserId = userId;
        Message = message;
        IsInternal = isInternal;
        CreatedAt = createdAt;
    }
}