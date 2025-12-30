using SmartTicketSystem.Application.DTOs;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface ITicketService
{
    Task<long> CreateAsync(CreateTicketDto dto, Guid guid);
    Task<TicketResponseDto> GetByIdAsync(long ticketId);
    Task<IEnumerable<TicketResponseDto>> GetAllAsync();
    Task<IEnumerable<TicketResponseDto>> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<TicketResponseDto>> GetByAssignedToIdAsync(Guid assignedToId);
    Task<bool> UpdateAsync(long ticketId, UpdateTicketDto dto, Guid modifiedBy);
    Task<bool> DeleteAsync(long ticketId);
}