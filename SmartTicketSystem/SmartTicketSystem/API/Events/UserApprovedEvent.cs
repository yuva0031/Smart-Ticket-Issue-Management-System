namespace SmartTicketSystem.API.Events;

public sealed class UserApprovedEvent : IDomainEvent
{
    public Guid UserId { get; }
    public Guid ApprovedBy { get; }
    public DateTime OccurredAt { get; }

    public UserApprovedEvent(Guid userId, Guid approvedBy)
    {
        UserId = userId;
        ApprovedBy = approvedBy;
        OccurredAt = DateTime.UtcNow;
    }
}