namespace SmartTicketSystem.Application.DTOs;

public class TicketHistoryResponseDto
{
    public int HistoryId { get; set; }
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string ModifiedByName { get; set; }
    public DateTime ChangedAt { get; set; }
}
