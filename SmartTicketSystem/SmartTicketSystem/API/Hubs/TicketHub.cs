using Microsoft.AspNetCore.SignalR;

using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Hubs;

public class TicketHub : Hub
{
    private readonly INotificationService _notificationService;

    public TicketHub(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            // Send any unread notifications to the newly connected user
            await SendUnreadNotifications(userId);
        }

        if (Context.User?.IsInRole("Admin") == true ||
            Context.User?.IsInRole("SupportManager") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "management-notifications");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        if (Context.User?.IsInRole("Admin") == true ||
            Context.User?.IsInRole("SupportManager") == true)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "management-notifications");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinTicketRoom(long ticketId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");

    public async Task LeaveTicketRoom(long ticketId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");

    private async Task SendUnreadNotifications(string userId)
    {
        try
        {
            if (Guid.TryParse(userId, out var userGuid))
            {
                var unreadNotifications = await _notificationService.GetUnreadByUserIdAsync(userGuid);

                if (unreadNotifications?.Any() == true)
                {
                    await Clients.Caller.SendAsync("UnreadNotifications", unreadNotifications);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending unread notifications: {ex.Message}");
        }
    }
}