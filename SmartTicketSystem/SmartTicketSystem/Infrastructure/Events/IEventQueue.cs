using SmartTicketSystem.API.Events;

namespace SmartTicketSystem.Infrastructure.Events;

public interface IEventQueue
{
    Task PublishAsync(IDomainEvent ev);
    IAsyncEnumerable<IDomainEvent> SubscribeAsync(CancellationToken token);
}
