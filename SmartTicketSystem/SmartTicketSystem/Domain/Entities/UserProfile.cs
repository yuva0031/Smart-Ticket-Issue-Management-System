namespace SmartTicketSystem.Domain.Entities
{
    public class UserProfile
    {
        public Guid UserProfileId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public string Address { get; set; }
        public User User { get; set; }
    }
}