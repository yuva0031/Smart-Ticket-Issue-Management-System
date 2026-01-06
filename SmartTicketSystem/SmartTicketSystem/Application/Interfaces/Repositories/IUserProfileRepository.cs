using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId);
    Task AddAsync(UserProfile profile);
    Task UpdateAsync(UserProfile userProfile);
    Task SaveAsync();
}