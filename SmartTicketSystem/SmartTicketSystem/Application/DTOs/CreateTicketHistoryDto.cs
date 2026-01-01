namespace SmartTicketSystem.Application.DTOs;

public class CreateTicketHistoryDto
{
    public CreateTicketHistoryDto(
        long ticketId,
        Guid modifiedBy,
        string fieldName,
        string oldValue,
        string newValue
    )
    {
        TicketId = ticketId;
        ModifiedBy = modifiedBy;
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public long TicketId { get; }
    public Guid ModifiedBy { get; }
    public string FieldName { get; }
    public string OldValue { get; }
    public string NewValue { get; }
}
