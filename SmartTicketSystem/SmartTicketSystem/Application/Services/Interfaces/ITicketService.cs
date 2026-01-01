using SmartTicketSystem.Application.DTOs;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface ITicketService
{
    Task<long> CreateAsync(CreateTicketDto dto, Guid ownerId);
    Task<TicketResponseDto?> GetTicketVisibleToUserAsync(long ticketId, Guid userId);
    Task<IEnumerable<TicketResponseDto>> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<TicketResponseDto>> GetByAssignedToIdAsync(Guid agentId);
    Task<IEnumerable<TicketResponseDto>> GetUnassignedTicketsAsync();
    Task<bool> UpdateAsync(long ticketId, UpdateTicketDto dto, Guid userId);
    Task<bool> DeleteAsync(long ticketId, Guid userId);
}
