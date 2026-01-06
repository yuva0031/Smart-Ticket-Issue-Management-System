using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}