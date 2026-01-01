using Microsoft.AspNetCore.SignalR;

using SmartTicketSystem.API.Events;
using SmartTicketSystem.API.Hubs;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Infrastructure.Events;

namespace SmartTicketSystem.Infrastructure.Workers;

public class TicketEventWorker : BackgroundService
{
    private readonly IEventQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<TicketHub> _hub;

    public TicketEventWorker(IEventQueue queue, IServiceScopeFactory scopeFactory, IHubContext<TicketHub> hub)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        Console.WriteLine("TicketEventWorker Started Listening...");

        await foreach (var ev in _queue.SubscribeAsync(token))
        {
            using var scope = _scopeFactory.CreateScope();

            var historyService = scope.ServiceProvider.GetRequiredService<ITicketHistoryService>();

            try
            {
                switch (ev)
                {
                    case TicketUpdatedEvent updated:
                        await HandleTicketUpdated(updated, historyService, token);
                        break;

                    case TicketCommentAddedEvent comment:
                        await HandleCommentAdded(comment, token);
                        break;

                    case TicketCommentUpdatedEvent updatedComment:
                        await HandleCommentUpdated(updatedComment, token);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Worker Error] {ex.Message}");
            }
        }
    }

    private async Task HandleTicketUpdated(TicketUpdatedEvent ev, ITicketHistoryService historyService, CancellationToken token)
    {
        foreach (var (field, change) in ev.Changes)
        {
            var parts = change.Split('|');
            if (parts.Length == 2)
                await historyService.LogChangeAsync(
                    new CreateTicketHistoryDto(
                        ev.TicketId,
                        ev.UpdatedBy,
                        field,
                        parts[0],
                        parts[1]
                    )
                );
        }

        await _hub.Clients.Group($"ticket-{ev.TicketId}")
            .SendAsync("TicketUpdated", new
            {
                ev.TicketId,
                ev.UpdatedBy,
                ev.Changes,
                UpdatedAt = DateTime.UtcNow
            }, token);
    }

    private async Task HandleCommentAdded(TicketCommentAddedEvent ev, CancellationToken token)
    {
        await _hub.Clients.Group($"ticket-{ev.TicketId}")
            .SendAsync("CommentAdded", new
            {
                ev.CommentId,
                ev.TicketId,
                ev.UserId,
                ev.Message,
                ev.IsInternal,
                ev.CreatedAt
            }, token);
    }

    private async Task HandleCommentUpdated(TicketCommentUpdatedEvent ev, CancellationToken token)
    {
        await _hub.Clients.Group($"ticket-{ev.TicketId}")
            .SendAsync("CommentUpdated", new
            {
                ev.CommentId,
                ev.TicketId,
                ev.UserId,
                ev.Message,
                ev.IsInternal,
                ev.UpdatedAt
            }, token);
    }
}