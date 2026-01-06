using AutoMapper;

using SmartTicketSystem.API.Events;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Infrastructure.Events;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

/// <summary>
/// Service for managing user administration, approval workflows, and agent discovery.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;
    private readonly IEventQueue _eventQueue;

    public UserService(IUserRepository repo, IMapper mapper, IEventQueue eventQueue)
    {
        _repo = repo;
        _mapper = mapper;
        _eventQueue = eventQueue;
    }

    /// <summary>
    /// Retrieves all users in the system along with their assigned roles.
    /// </summary>
    /// <returns>A collection of UserDto objects.</returns>
    public async Task<IEnumerable<UserDto>> GetAllUsersWithRolesAsync()
    {
        var users = await _repo.GetAllUsersAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    /// <summary>
    /// Retrieves a specific user by their unique identifier.
    /// </summary>
    /// <param name="id">The Guid of the user.</param>
    /// <returns>A UserDto if found; otherwise, null.</returns>
    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _repo.GetByIdAsync(id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    /// <summary>
    /// Retrieves support agents, optionally filtered by a specific category skill.
    /// </summary>
    /// <param name="categoryId">Optional category ID to filter agents by skill.</param>
    /// <returns>A collection of support agents.</returns>
    public async Task<IEnumerable<UserDto>> GetSupportAgentsAsync(int? categoryId = null)
    {
        var users = await _repo.GetSupportAgentsAsync(categoryId);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    /// <summary>
    /// Retrieves all users who are currently inactive and awaiting admin approval.
    /// </summary>
    /// <returns>A collection of pending users.</returns>
    public async Task<IEnumerable<UserDto>> GetPendingUsersAsync()
    {
        var users = await _repo.GetInactiveUsersAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    /// <summary>
    /// Approves a pending user, activating their account and publishing an approval event.
    /// </summary>
    /// <param name="userId">The ID of the user to approve.</param>
    /// <param name="approvedBy">The ID of the administrator performing the approval.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
    public async Task ApproveUserAsync(Guid userId, Guid approvedBy)
    {
        var user = await _repo.GetByIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        if (user.IsActive)
            return;

        user.IsActive = true;

        await _repo.UpdateAsync(user);
        await _repo.SaveAsync();

        await _eventQueue.PublishAsync(new UserApprovedEvent(user.Id, approvedBy));
    }
}