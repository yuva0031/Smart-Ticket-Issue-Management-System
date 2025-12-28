using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> GetByEmail(string email);
    Task<User?> GetByEmailWithRoles(string email);
    Task AddUser(User user);
    Task Save();
}