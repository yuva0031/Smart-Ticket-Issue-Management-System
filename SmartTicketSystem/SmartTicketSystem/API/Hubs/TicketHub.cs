using System.Text.RegularExpressions;

using Microsoft.AspNetCore.SignalR;

namespace SmartTicketSystem.API.Hubs;

public class TicketHub : Hub
{
    public async Task JoinTicketRoom(long ticketId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");

    public async Task LeaveTicketRoom(long ticketId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
}
