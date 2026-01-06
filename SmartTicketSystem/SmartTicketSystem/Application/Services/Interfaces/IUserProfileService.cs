using SmartTicketSystem.Application.DTOs;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDto> GetMyProfileAsync(Guid userId);
    Task UpdateMyProfileAsync(Guid userId, UpdateUserProfileDto dto);
}