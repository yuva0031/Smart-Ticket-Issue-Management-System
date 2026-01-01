using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Application.DTOs.AddTicketCommentDto;

public class AddCommentRequest
{
    public string Message { get; set; }
    public bool IsInternal { get; set; }
}