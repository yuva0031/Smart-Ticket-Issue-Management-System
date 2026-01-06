using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Enums;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

public class LookupService : ILookupService
{
    public Task<IEnumerable<LookupItemDto>> GetRolesAsync() =>
        Task.FromResult(ToLookup<UserRoleEnum>());

    public Task<IEnumerable<LookupItemDto>> GetStatusesAsync() =>
        Task.FromResult(ToLookup<TicketStatusEnum>());

    public Task<IEnumerable<LookupItemDto>> GetPrioritiesAsync() =>
        Task.FromResult(ToLookup<TicketPriorityEnum>());

    public Task<IEnumerable<LookupItemDto>> GetCategoriesAsync() =>
        Task.FromResult(ToLookup<TicketCategoryEnum>());

    private static IEnumerable<LookupItemDto> ToLookup<TEnum>()
        where TEnum : struct, Enum 
    {
        return Enum.GetValues<TEnum>()
            .Select(e => new LookupItemDto
            {
                Id = Convert.ToInt32(e),
                Code = e.ToString(),
                Name = FormatName(e.ToString())
            });
    }

    private static string FormatName(string value)
    {
        return System.Text.RegularExpressions.Regex
            .Replace(value, "([A-Z])", " $1")
            .Trim();
    }
}
