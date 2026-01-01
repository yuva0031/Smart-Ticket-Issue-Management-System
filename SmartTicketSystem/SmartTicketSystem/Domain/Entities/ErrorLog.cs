using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Domain.Entities;

public class ErrorLog
{
    public int ErrorId { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}