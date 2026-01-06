using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.DTOs.AddTicketCommentDto;
using SmartTicketSystem.Application.Services.Interfaces;

namespace SmartTicketSystem.API.Controllers;

/// <summary>
/// Controller for managing interactions and discussions within support tickets.
/// </summary>
[ApiController]
[Route("api/tickets")]
[Authorize] // Base authorization; roles specified at endpoint level
public class TicketCommentController : ControllerBase
{
    private readonly ITicketCommentService _ticketCommentService;

    public TicketCommentController(ITicketCommentService service)
    {
        _ticketCommentService = service;
    }

    /// <summary>
    /// Adds a new comment to a specific ticket.
    /// </summary>
    /// <param name="ticketId">The unique ID of the ticket.</param>
    /// <param name="dto">The comment content and visibility settings.</param>
    /// <returns>The generated Snowflake ID for the new comment.</returns>
    /// <response code="200">Comment successfully added.</response>
    /// <response code="400">If the request data is invalid.</response>
    [HttpPost("{ticketId}/comments")]
    [Authorize(Roles = "SupportAgent, SupportManager, EndUser")]
    public async Task<IActionResult> AddComment(long ticketId, [FromBody] AddCommentRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();
            long commentId = await _ticketCommentService.AddCommentAsync(dto, userId, ticketId);

            return Ok(new
            {
                Message = "Comment added successfully",
                CommentId = commentId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while adding the comment.", Details = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all comments associated with a specific ticket.
    /// </summary>
    /// <param name="ticketId">The unique ID of the ticket.</param>
    /// <returns>A collection of comment details.</returns>
    [HttpGet("{ticketId}/comments")]
    [Authorize(Roles = "SupportAgent, SupportManager, EndUser")]
    public async Task<IActionResult> GetComments(long ticketId)
    {
        var comments = await _ticketCommentService.GetAllByTicketId(ticketId);
        return Ok(comments);
    }

    /// <summary>
    /// Updates an existing comment's message or internal visibility.
    /// </summary>
    /// <param name="commentId">The ID of the comment to update.</param>
    /// <param name="dto">The updated content.</param>
    /// <returns>A status message.</returns>
    /// <response code="200">Comment updated successfully.</response>
    /// <response code="404">If the comment does not exist.</response>
    [HttpPut("comments/{commentId}")]
    [Authorize(Roles = "SupportAgent, SupportManager, EndUser")]
    public async Task<IActionResult> UpdateComment(long commentId, [FromBody] UpdateCommentRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            bool success = await _ticketCommentService.UpdateCommentAsync(commentId, dto.Message, dto.IsInternal);

            if (!success)
                return NotFound(new { Message = "Comment not found" });

            return Ok(new { Message = "Comment updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while updating the comment.", Details = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(idClaim))
            throw new UnauthorizedAccessException("User ID claim not found in token.");

        return Guid.Parse(idClaim);
    }
}