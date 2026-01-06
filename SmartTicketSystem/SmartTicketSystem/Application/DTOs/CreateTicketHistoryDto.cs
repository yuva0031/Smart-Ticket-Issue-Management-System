namespace SmartTicketSystem.Application.DTOs;

public class CreateTicketHistoryDto
{
    public CreateTicketHistoryDto() { }

    public CreateTicketHistoryDto(long ticketId, Guid modifiedBy, string fieldName, string oldValue, string newValue)
    {
        TicketId = ticketId;
        ModifiedBy = modifiedBy;
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public long TicketId { get; set; }
    public Guid ModifiedBy { get; set; }
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
}