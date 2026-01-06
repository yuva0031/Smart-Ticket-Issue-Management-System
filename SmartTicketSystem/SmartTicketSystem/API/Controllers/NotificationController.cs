using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.API.Controllers;

/// <summary>
/// Controller for managing user notifications, including unread counts and status updates.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves all unread notifications for the currently authenticated user.
    /// </summary>
    /// <returns>A collection of unread notifications.</returns>
    /// <response code="200">Unread notifications retrieved successfully.</response>
    [HttpGet("unread")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetUnread()
    {
        var userId = GetCurrentUserId();
        var notifications = await _service.GetUnreadByUserIdAsync(userId);
        return Ok(notifications);
    }

    /// <summary>
    /// Retrieves a paginated list of all notifications for the authenticated user.
    /// </summary>
    /// <param name="pageNumber">The page index.</param>
    /// <param name="pageSize">Items per page.</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notification>>> GetMyNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var notifications = await _service.GetByUserIdAsync(userId, pageNumber, pageSize);
        return Ok(notifications);
    }

    /// <summary>
    /// Marks a specific notification as read.
    /// </summary>
    /// <param name="id">The unique Snowflake ID of the notification.</param>
    /// <response code="200">Notification marked as read.</response>
    /// <response code="404">Notification not found.</response>
    [HttpPatch("{id:long}/read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var success = await _service.MarkAsReadAsync(id);
        if (!success) return NotFound(new { Message = "Notification not found" });

        return Ok(new { Message = "Notification marked as read" });
    }

    /// <summary>
    /// Marks all unread notifications for the authenticated user as read.
    /// </summary>
    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetCurrentUserId();
        await _service.MarkAllAsReadAsync(userId);
        return Ok(new { Message = "All notifications marked as read" });
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue("id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(idClaim))
            throw new UnauthorizedAccessException("User identification claim missing.");

        return Guid.Parse(idClaim);
    }
}