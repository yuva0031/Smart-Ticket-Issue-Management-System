using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

/// <summary>
/// Controller for managing the authenticated user's personal profile and contact information.
/// </summary>
[ApiController]
[Route("api/user-profile")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _service;

    public UserProfileController(IUserProfileService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves the profile details for the currently authenticated user.
    /// </summary>
    /// <returns>The user's profile information.</returns>
    /// <response code="200">Profile retrieved successfully.</response>
    /// <response code="404">Profile not found for the current user.</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _service.GetMyProfileAsync(userId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while fetching the profile.", Details = ex.Message });
        }
    }

    /// <summary>
    /// Updates the profile information for the currently authenticated user.
    /// </summary>
    /// <param name="dto">The updated profile details.</param>
    /// <returns>A success message.</returns>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">If the input data is invalid.</response>
    [HttpPut]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();
            await _service.UpdateMyProfileAsync(userId, dto);
            return Ok(new { Message = "Profile updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while updating the profile.", Details = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue("id");
        if (string.IsNullOrEmpty(idClaim))
            throw new UnauthorizedAccessException("User identification claim missing from token.");

        return Guid.Parse(idClaim);
    }
}