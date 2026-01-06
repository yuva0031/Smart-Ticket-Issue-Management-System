using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("agents")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetSupportAgents([FromQuery] int? categoryId)
    {
        var agents = await _userService.GetSupportAgentsAsync(categoryId);
        return Ok(agents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(user);
    }

    [Authorize(Roles = "EndUser, SupportAgent, SupportManager, Admin")]
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllEndUsers()
    {
        var users = await _userService.GetAllUsersWithRolesAsync();
        return Ok(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersWithRolesAsync();
        return Ok(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pendingapprovals")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetPendingUsers()
    {
        var users = await _userService.GetPendingUsersAsync();
        return Ok(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("approve-user/{userId}")]
    public async Task<IActionResult> ApproveUser(Guid userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("id");

        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("User ID claim not found");

        if (!Guid.TryParse(userIdClaim, out Guid approvedBy))
            return BadRequest("Invalid Admin ID format");

        await _userService.ApproveUserAsync(userId, approvedBy);
        return Ok(new { message = "User approved successfully" });
    }
}