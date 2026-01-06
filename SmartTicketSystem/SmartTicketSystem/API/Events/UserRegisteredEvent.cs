namespace SmartTicketSystem.API.Events;

public sealed class UserRegisteredEvent : IDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string FullName { get; }
    public int RoleId { get; }
    public bool RequiresApproval { get; }
    public DateTime OccurredAt { get; }

    public UserRegisteredEvent(
        Guid userId,
        string email,
        string fullName,
        int roleId,
        bool requiresApproval)
    {
        UserId = userId;
        Email = email;
        FullName = fullName;
        RoleId = roleId;
        RequiresApproval = requiresApproval;
        OccurredAt = DateTime.UtcNow;
    }
}