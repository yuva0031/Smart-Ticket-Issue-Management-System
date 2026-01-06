using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface ITicketPriorityService
{
    Task<IEnumerable<TicketPriorityDto>> GetAllAsync();
    Task<TicketPriority?> GetPriorityByIdAsync(int priorityId);
    Task<bool> UpdateSlaAsync(int priorityId, int SLAHours);
}