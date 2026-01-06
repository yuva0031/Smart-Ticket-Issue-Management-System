using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetSupportAgentsAsync(int? categoryId = null);
    Task<IEnumerable<User>> GetInactiveUsersAsync();
    Task UpdateAsync(User user);
    Task SaveAsync();
}