using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

[ApiController]
[Route("api/lookups")]
[AllowAnonymous]
public class LookupController : ControllerBase
{
    private readonly ILookupService _service;

    public LookupController(ILookupService service)
    {
        _service = service;
    }

    [HttpGet("roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _service.GetRolesAsync();
        return Ok(roles);
    }

    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _service.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("priorities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPriorities()
    {
        var priorities = await _service.GetPrioritiesAsync();
        return Ok(priorities);
    }

    [HttpGet("statuses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatuses()
    {
        var statuses = await _service.GetStatusesAsync();
        return Ok(statuses);
    }
}
