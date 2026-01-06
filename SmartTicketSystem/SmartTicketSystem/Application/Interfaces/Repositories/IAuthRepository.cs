using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface IAuthRepository
{
    Task<User> GetByEmail(string email);
    Task<User?> GetByEmailWithRole(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAgentProfile(AgentProfile profile);
    Task AddUser(User user);
    Task<List<Guid>> GetUserIdsByRoles(int[] roleIds);

    Task Save();
}