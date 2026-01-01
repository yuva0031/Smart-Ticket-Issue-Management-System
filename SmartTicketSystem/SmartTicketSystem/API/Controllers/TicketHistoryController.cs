using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

[ApiController]
[Route("api/tickets/{ticketId}/history")]
public class TicketHistoryController : ControllerBase
{
    private readonly ITicketHistoryService _service;

    public TicketHistoryController(ITicketHistoryService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetHistory(long ticketId)
    {
        var logs = await _service.GetHistoryAsync(ticketId);
        return Ok(logs);
    }
}