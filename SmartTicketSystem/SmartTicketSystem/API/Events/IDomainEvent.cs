namespace SmartTicketSystem.API.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}