using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.DTOs.AddTicketCommentDto;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.API.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketCommentController : ControllerBase
{
    private readonly ITicketCommentService _ticketCommentService;

    public TicketCommentController(ITicketCommentService service)
    {
        _ticketCommentService = service;
    }

    [Authorize]
    [HttpPost("{ticketId}/comments")]
    public async Task<IActionResult> AddComment(long ticketId, [FromBody] AddCommentRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirst("id")!.Value);

        long commentId = await _ticketCommentService.AddCommentAsync(dto, userId, ticketId);

        return Ok(new
        {
            Message = "Comment added successfully",
            CommentId = commentId
        });
    }

    [Authorize]
    [HttpGet("{ticketId}/comments")]
    public async Task<IActionResult> GetComments(long ticketId)
    {
        var comments = await _ticketCommentService.GetAllByTicketId(ticketId);
        return Ok(comments);
    }

    [Authorize]
    [HttpPut("comments/{commentId}")]
    public async Task<IActionResult> UpdateComment(long commentId, [FromBody] UpdateCommentRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirst("id")!.Value);

        bool success = await _ticketCommentService.UpdateCommentAsync(commentId, dto.Message, dto.IsInternal);

        if (!success)
            return NotFound(new { Message = "Comment not found" });

        return Ok(new { Message = "Comment updated successfully" });
    }
}