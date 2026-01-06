using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Services.Interfaces;
public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersWithRolesAsync();
    Task<IEnumerable<UserDto>> GetSupportAgentsAsync(int? categoryId);
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<UserDto>> GetPendingUsersAsync();
    Task ApproveUserAsync(Guid userId, Guid approvedBy);
}