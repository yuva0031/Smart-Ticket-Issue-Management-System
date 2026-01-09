using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly IAgentService _service;

    public AgentController(IAgentService service)
    {
        _service = service;
    }

    [HttpGet("my-profile")]
    [Authorize(Roles = "SupportAgent")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        var profile = await _service.GetAgentProfileByUserId(userId);

        if (profile == null)
            return NotFound(new { Message = "Agent profile not found" });

        return Ok(profile);
    }

    [HttpGet("profile/{agentProfileId:guid}")]
    [Authorize(Roles = "Admin,SupportManager")]
    public async Task<IActionResult> GetAgentProfile(Guid agentProfileId)
    {
        var profile = await _service.GetAgentProfileById(agentProfileId);

        if (profile == null)
            return NotFound(new { Message = "Agent profile not found" });

        return Ok(profile);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,SupportManager")]
    public async Task<IActionResult> GetAllAgentProfiles()
    {
        var profiles = await _service.GetAllAgentProfiles();
        return Ok(profiles);
    }

    [HttpPost("add-skill")]
    [Authorize(Roles = "Admin,SupportAgent")]
    public async Task<IActionResult> AddSkill([FromBody] AddAgentSkillDto dto)
    {
        var result = await _service.AddCategorySkill(dto);
        return Ok(new { Message = result });
    }

    [HttpDelete("remove-skill")]
    [Authorize(Roles = "Admin,SupportAgent")]
    public async Task<IActionResult> RemoveSkill([FromBody] RemoveAgentSkillDto dto)
    {
        var result = await _service.RemoveCategorySkill(dto);
        return Ok(new { Message = result });
    }

    [HttpPost("update-my-skills")]
    [Authorize(Roles = "SupportAgent")]
    public async Task<IActionResult> UpdateMySkills([FromBody] UpdateMySkillsDto dto)
    {
        var userId = GetUserId();
        var result = await _service.UpdateAgentSkills(userId, dto.CategoryIds);

        return Ok(new { Message = result });
    }
    private Guid GetUserId()
    {
        var id = User.FindFirstValue("id");
        if (string.IsNullOrEmpty(id))
            throw new UnauthorizedAccessException("User ID missing in token");

        return Guid.Parse(id);
    }
}
