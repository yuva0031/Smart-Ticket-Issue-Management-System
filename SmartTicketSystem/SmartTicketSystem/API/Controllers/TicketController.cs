using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _service;

    public TicketController(ITicketService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = "EndUser")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue("id"));
        var ticketId = await _service.CreateAsync(dto, userId);

        return Ok(new { Message = "Ticket created successfully", TicketId = ticketId });
    }

    [HttpGet("{ticketId:long}")]
    [Authorize(Roles = "EndUser,SupportAgent,SupportManager,Admin")]
    public async Task<IActionResult> GetTicketById(long ticketId)
    {
        var userId = Guid.Parse(User.FindFirstValue("id"));
        var result = await _service.GetTicketVisibleToUserAsync(ticketId, userId);

        return result is null ? NotFound("Ticket not found or access denied") : Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "EndUser")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = Guid.Parse(User.FindFirstValue("id"));
        return Ok(await _service.GetByOwnerIdAsync(userId));
    }

    [HttpGet("assigned")]
    [Authorize(Roles = "SupportAgent")]
    public async Task<IActionResult> GetAssignedToMe()
    {
        var userId = Guid.Parse(User.FindFirstValue("id"));
        return Ok(await _service.GetByAssignedToIdAsync(userId));
    }

    [HttpGet("unassigned")]
    [Authorize(Roles = "SupportManager,Admin")]
    public async Task<IActionResult> GetUnassignedTickets()
    {
        return Ok(await _service.GetUnassignedTicketsAsync());
    }

    [HttpPut("{ticketId:long}")]
    [Authorize(Roles = "EndUser,SupportAgent,SupportManager")]
    public async Task<IActionResult> UpdateTicket(long ticketId, [FromBody] UpdateTicketDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue("id"));
        var updated = await _service.UpdateAsync(ticketId, dto, userId);

        return updated ? Ok("Ticket updated successfully") : Forbid("Not allowed to update this ticket");
    }

    [HttpDelete("{ticketId:long}")]
    [Authorize(Roles = "EndUser,SupportManager")]
    public async Task<IActionResult> DeleteTicket(long ticketId)
    {
        var userId = Guid.Parse(User.FindFirstValue("id"));
        var result = await _service.DeleteAsync(ticketId, userId);

        return result ? Ok("Ticket deleted") : Forbid("Not allowed or ticket not found");
    }
}
