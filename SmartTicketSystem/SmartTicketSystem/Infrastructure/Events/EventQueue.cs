using System.Threading.Channels;

using SmartTicketSystem.API.Events;

namespace SmartTicketSystem.Infrastructure.Events;

public class EventQueue : IEventQueue
{
    private readonly Channel<IDomainEvent> _channel =
        Channel.CreateUnbounded<IDomainEvent>();

    public Task PublishAsync(IDomainEvent ev)
        => _channel.Writer.WriteAsync(ev).AsTask();

    public IAsyncEnumerable<IDomainEvent> SubscribeAsync(CancellationToken token)
        => _channel.Reader.ReadAllAsync(token);
}