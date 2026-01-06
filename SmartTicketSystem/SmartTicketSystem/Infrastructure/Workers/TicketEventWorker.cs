using Microsoft.AspNetCore.SignalR;

using SmartTicketSystem.API.Events;
using SmartTicketSystem.API.Hubs;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Domain.Enums;
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
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var authRepository = scope.ServiceProvider.GetRequiredService<IAuthRepository>();
            var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();

            try
            {
                switch (ev)
                {
                    case TicketUpdatedEvent updated:
                        await HandleTicketUpdated(updated, historyService, notificationService, ticketRepository, token);
                        break;

                    case TicketCommentAddedEvent comment:
                        await HandleCommentAdded(comment, token);
                        break;

                    case TicketCommentUpdatedEvent updatedComment:
                        await HandleCommentUpdated(updatedComment, token);
                        break;

                    case UserRegisteredEvent registered:
                        await HandleUserRegistered(registered, notificationService, authRepository, token);
                        break;

                    case UserApprovedEvent approved:
                        await HandleUserApproved(approved, notificationService, token);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Worker Error] {ex.Message}");
            }
        }
    }

    private async Task HandleTicketUpdated(TicketUpdatedEvent ev, ITicketHistoryService historyService, INotificationService notificationService,
    ITicketRepository ticketRepository, CancellationToken token)
    {
        foreach (var (field, change) in ev.Changes)
        {
            var parts = change.Split('|');
            if (parts.Length == 2)
                await historyService.LogChangeAsync(
                    new CreateTicketHistoryDto(ev.TicketId, ev.UpdatedBy, field, parts[0], parts[1])
                );
        }

        var ticket = await ticketRepository.GetByIdAsync(ev.TicketId);
        if (ticket != null && ticket.OwnerId != ev.UpdatedBy)
        {
            await notificationService.CreateAsync(new Notification
            {
                UserId = ticket.OwnerId,
                Message = $"Your ticket #{ev.TicketId} has been updated.",
                Channel = NotificationChannelEnum.InApp
            });
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
                ev.OccurredAt
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

    private async Task HandleUserRegistered(
        UserRegisteredEvent ev,
        INotificationService notificationService,
        IAuthRepository authRepository,
        CancellationToken token)
    {
        Console.WriteLine($"Handling user registration for: {ev.FullName}");

        var adminManagerIds = await GetAdminAndManagerUserIds(authRepository);

        foreach (var adminId in adminManagerIds)
        {
            await notificationService.CreateAsync(new Notification
            {
                UserId = adminId,
                Message = $"New user registered: {ev.FullName} ({ev.Email})" +
                          (ev.RequiresApproval ? " - Requires approval" : ""),
                Channel = NotificationChannelEnum.InApp
            });
        }

        try
        {
            await _hub.Clients.Group("management-notifications")
                .SendAsync("UserRegistered", new
                {
                    ev.UserId,
                    ev.FullName,
                    ev.Email,
                    ev.RoleId,
                    ev.RequiresApproval,
                    ev.OccurredAt
                }, token);

            Console.WriteLine("Real-time SignalR notification sent to connected admins/managers");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR notification failed (users may be offline): {ex.Message}");
        }
    }

    private async Task HandleUserApproved(UserApprovedEvent ev, INotificationService notificationService, CancellationToken token)
    {
        await notificationService.CreateAsync(new Notification
        {
            UserId = ev.UserId,
            Message = "Your account has been approved. You can now log in.",
            Channel = NotificationChannelEnum.InApp
        });

        await _hub.Clients
            .Group($"user-{ev.UserId}")
            .SendAsync("AccountActivated", new
            {
                Message = "Your account has been approved.",
                ev.OccurredAt
            }, token);

        await _hub.Clients
            .Group("management-notifications")
            .SendAsync("UserApproved", new
            {
                ev.UserId,
                ev.ApprovedBy,
                ev.OccurredAt
            }, token);
    }

    private async Task<List<Guid>> GetAdminAndManagerUserIds(IAuthRepository authRepository)
    {
        return await authRepository.GetUserIdsByRoles(new[]
        {
            (int)UserRoleEnum.Admin,
            (int)UserRoleEnum.SupportManager
        });
    }
}