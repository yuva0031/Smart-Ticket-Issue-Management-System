using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

/// <summary>
/// Controller for accessing the audit trail and change history of support tickets.
/// </summary>
[ApiController]
[Route("api/tickets/{ticketId}/history")]
[Authorize]
public class TicketHistoryController : ControllerBase
{
    private readonly ITicketHistoryService _service;

    public TicketHistoryController(ITicketHistoryService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves the chronological history of changes for a specific ticket.
    /// </summary>
    /// <param name="ticketId">The unique identifier of the ticket.</param>
    /// <returns>A collection of historical log entries.</returns>
    /// <response code="200">History retrieved successfully.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error during retrieval.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketHistoryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(long ticketId)
    {
        try
        {
            var logs = await _service.GetHistoryAsync(ticketId);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            // Logging should be implemented here
            return StatusCode(500, new { Message = "An error occurred while retrieving ticket history.", Details = ex.Message });
        }
    }
}