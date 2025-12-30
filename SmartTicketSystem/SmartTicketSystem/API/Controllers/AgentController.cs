using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Infrastructure.Services.Implementations;

namespace SmartTicketSystem.API.Controllers;

[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly AgentService _service;

    public AgentController(AgentService service)
    {
        _service = service;
    }

    [HttpPost("add-skill")]
    [Authorize(Roles = "Admin,SupportManager")]
    public async Task<IActionResult> AddSkill([FromBody] AddAgentSkillDto dto)
        => Ok(await _service.AddCategorySkill(dto));

    [HttpDelete("remove-skill")]
    [Authorize(Roles = "Admin,SupportManager")]
    public async Task<IActionResult> RemoveSkill([FromBody] RemoveAgentSkillDto dto)
        => Ok(await _service.RemoveCategorySkill(dto));
}