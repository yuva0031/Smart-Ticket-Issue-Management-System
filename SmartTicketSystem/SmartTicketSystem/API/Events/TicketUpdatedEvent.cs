namespace SmartTicketSystem.API.Events;

public sealed class TicketUpdatedEvent : IDomainEvent
{
    public long TicketId { get; }
    public Guid UpdatedBy { get; }
    public Dictionary<string, string> Changes { get; }

    public TicketUpdatedEvent(long ticketId, Guid updatedBy, Dictionary<string, string> changes)
    {
        TicketId = ticketId;
        UpdatedBy = updatedBy;
        Changes = changes;
    }
}