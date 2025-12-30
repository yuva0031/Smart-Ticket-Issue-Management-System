namespace SmartTicketSystem.Application.DTOs;

public class AddTicketHistoryDto
{
    public long TicketId { get; set; }
    public Guid ModifiedBy { get; set; }
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
}