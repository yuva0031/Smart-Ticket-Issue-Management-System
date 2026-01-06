using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

/// <summary>
/// Controller for managing ticket priority levels and their associated Service Level Agreements (SLA).
/// Only accessible by users with the SupportManager role.
/// </summary>
[ApiController]
[Route("api/ticket-priorities")]
[Authorize(Roles = "SupportManager")]
public class TicketPriorityController : ControllerBase
{
    private readonly ITicketPriorityService _service;

    public TicketPriorityController(ITicketPriorityService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves all available ticket priority levels and their SLA configurations.
    /// </summary>
    /// <returns>A collection of ticket priority details.</returns>
    /// <response code="200">Priorities retrieved successfully.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="403">Forbidden - Requires SupportManager role.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketPriorityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var priorities = await _service.GetAllAsync();
        return Ok(priorities);
    }

    /// <summary>
    /// Updates the SLA (Service Level Agreement) duration in hours for a specific priority.
    /// </summary>
    /// <param name="priorityId">The unique ID of the priority level.</param>
    /// <param name="slaHours">The new duration in hours.</param>
    /// <returns>A success message or error details.</returns>
    /// <response code="200">SLA updated successfully.</response>
    /// <response code="404">If the priority ID does not exist.</response>
    [HttpPut("{priorityId:int}/sla/{slaHours:int}")]
    public async Task<IActionResult> UpdateSla(int priorityId, int slaHours)
    {
        try
        {
            var updated = await _service.UpdateSlaAsync(priorityId, slaHours);

            if (!updated)
                return NotFound(new { Message = "Priority not found" });

            return Ok(new { Message = "SLA updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while updating the SLA.", Details = ex.Message });
        }
    }
}