using SmartTicketSystem.Application.DTOs;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface ILookupService
{
    Task<IEnumerable<LookupItemDto>> GetRolesAsync();
    Task<IEnumerable<LookupItemDto>> GetStatusesAsync();
    Task<IEnumerable<LookupItemDto>> GetPrioritiesAsync();
    Task<IEnumerable<LookupItemDto>> GetCategoriesAsync();
}