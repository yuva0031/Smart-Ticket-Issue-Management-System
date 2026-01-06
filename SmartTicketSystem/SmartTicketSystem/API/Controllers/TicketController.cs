using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

/// <summary>
/// Controller responsible for managing the support ticket lifecycle, including creation, 
/// status updates, assignments, and deletions.
/// </summary>
[ApiController]
[Route("api/tickets")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _service;

    public TicketController(ITicketService service)
    {
        _service = service;
    }

    /// <summary>
    /// Creates a new support ticket for the authenticated EndUser.
    /// </summary>
    /// <param name="dto">The ticket details.</param>
    /// <returns>A success message and the new Ticket ID.</returns>
    /// <response code="200">Ticket created successfully.</response>
    /// <response code="400">If the input data is invalid.</response>
    [HttpPost]
    [Authorize(Roles = "EndUser")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        var ticketId = await _service.CreateAsync(dto, userId);

        return Ok(new { Message = "Ticket created successfully", TicketId = ticketId });
    }

    /// <summary>
    /// Retrieves a specific ticket if the user has permission to view it.
    /// </summary>
    [HttpGet("{ticketId:long}")]
    [Authorize(Roles = "EndUser,SupportAgent,SupportManager")]
    public async Task<IActionResult> GetTicketById(long ticketId)
    {
        var userId = GetCurrentUserId();
        var result = await _service.GetTicketVisibleToUserAsync(ticketId, userId);

        return result is null
            ? NotFound(new { Message = "Ticket not found or access denied" })
            : Ok(result);
    }

    /// <summary>
    /// Retrieves all tickets created by the currently authenticated user.
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "EndUser")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = GetCurrentUserId();
        return Ok(await _service.GetByOwnerIdAsync(userId));
    }

    /// <summary>
    /// Retrieves all tickets in the system (Manager only).
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "SupportManager")]
    public async Task<IActionResult> GetAllTickets()
    {
        return Ok(await _service.GetAllTicketsAsync());
    }

    /// <summary>
    /// Retrieves tickets currently assigned to the authenticated Support Agent.
    /// </summary>
    [HttpGet("assigned")]
    [Authorize(Roles = "SupportAgent")]
    public async Task<IActionResult> GetAssignedToMe()
    {
        var userId = GetCurrentUserId();
        return Ok(await _service.GetByAssignedToIdAsync(userId));
    }

    /// <summary>
    /// Retrieves all tickets that have not yet been assigned to an agent.
    /// </summary>
    [HttpGet("unassigned")]
    [Authorize(Roles = "SupportManager")]
    public async Task<IActionResult> GetUnassignedTickets()
    {
        return Ok(await _service.GetUnassignedTicketsAsync());
    }

    /// <summary>
    /// Updates an existing ticket. Description is updatable by owners; 
    /// full fields are updatable by staff.
    /// </summary>
    [HttpPut("{ticketId:long}")]
    [Authorize(Roles = "SupportAgent,SupportManager,EndUser")]
    public async Task<IActionResult> UpdateTicket(long ticketId, [FromBody] UpdateTicketRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        var updated = await _service.UpdateAsync(ticketId, dto, userId);

        if (!updated)
            return Forbid("Not allowed to update this ticket or ticket not found");

        return Ok(new { Message = "Ticket updated successfully" });
    }

    /// <summary>
    /// Deletes a ticket. Only owners can delete unassigned tickets.
    /// </summary>
    [HttpDelete("{ticketId:long}")]
    [Authorize(Roles = "EndUser,SupportManager")]
    public async Task<IActionResult> DeleteTicket(long ticketId)
    {
        var userId = GetCurrentUserId();
        var result = await _service.DeleteAsync(ticketId, userId);

        return result
            ? Ok(new { Message = "Ticket deleted" })
            : Forbid("Not allowed to delete this ticket or ticket not found");
    }

    private Guid GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue("id");
        if (string.IsNullOrEmpty(claimValue))
            throw new UnauthorizedAccessException("User identification claim missing.");

        return Guid.Parse(claimValue);
    }
}