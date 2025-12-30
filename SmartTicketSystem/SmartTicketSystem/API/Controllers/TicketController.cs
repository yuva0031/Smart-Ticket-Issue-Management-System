using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Services.Implementations;

namespace SmartTicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _service;

    public TicketController(ITicketService service)
    {
        _service = service;
    }

    [HttpPost("create")]
    [Authorize(Roles = "EndUser")]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ownerId = Guid.Parse(User.FindFirstValue("id"));
        var ticketId = await _service.CreateAsync(dto, ownerId);

        return Ok(new { Message = "Ticket Created", TicketId = ticketId });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetTicketById(long id)
    {
        var ticket = await _service.GetByIdAsync(id);
        return ticket == null ? NotFound("Ticket not found") : Ok(ticket);
    }

    [HttpGet("all")]
    [Authorize(Roles = "SupportAgent")]
    public async Task<IActionResult> GetAllTickets()
    {
        var userId = Guid.Parse(User.FindFirstValue("id"));
        return Ok(await _service.GetByOwnerIdAsync(userId));
    }

    [HttpGet("assigned")]
    [Authorize(Roles = "Agent,SupportManager,SeniorAgent,Admin")]
    public async Task<IActionResult> GetByAssignedToId()
    {
        var userId = Guid.Parse(User.FindFirstValue("id"));
        return Ok(await _service.GetByAssignedToIdAsync(userId));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Agent,SupportManager,Admin")]
    public async Task<IActionResult> Update(long id, UpdateTicketDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue("id"));
        var result = await _service.UpdateAsync(id, dto, userId);
        return result ? Ok("Updated") : NotFound("Ticket not found");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SupportManager,Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? Ok("Ticket Deleted") : NotFound("Ticket not found");
    }
}