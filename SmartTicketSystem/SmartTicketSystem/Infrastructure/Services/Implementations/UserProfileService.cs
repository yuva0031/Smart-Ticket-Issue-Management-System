using AutoMapper;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

/// <summary>
/// Service responsible for managing user-specific profile information.
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repo;
    private readonly IMapper _mapper;

    public UserProfileService(IUserProfileRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves the profile details for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A DTO containing user profile information.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the profile does not exist.</exception>
    public async Task<UserProfileDto> GetMyProfileAsync(Guid userId)
    {
        var profile = await _repo.GetByUserIdAsync(userId);

        if (profile == null)
            throw new KeyNotFoundException($"Profile for User ID {userId} not found");

        return _mapper.Map<UserProfileDto>(profile);
    }

    /// <summary>
    /// Updates the contact information for an existing user profile.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="dto">The updated profile details.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the profile does not exist.</exception>
    public async Task UpdateMyProfileAsync(Guid userId, UpdateUserProfileDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var profile = await _repo.GetByUserIdAsync(userId);

        if (profile == null)
            throw new KeyNotFoundException($"Profile for User ID {userId} not found");

        profile.PhoneNumber = dto.PhoneNumber;
        profile.Address = dto.Address;

        await _repo.UpdateAsync(profile);
        await _repo.SaveAsync();
    }
}