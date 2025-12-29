using System.ComponentModel.DataAnnotations;

namespace SmartTicketSystem.Domain.Entities;

public class RefreshToken
{
    [Key]
    public int TokenId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }
    public User User { get; set; }
}