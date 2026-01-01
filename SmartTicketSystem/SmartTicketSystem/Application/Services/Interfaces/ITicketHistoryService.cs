using SmartTicketSystem.Application.DTOs;

namespace SmartTicketSystem.Application.Services.Interfaces;

public interface ITicketHistoryService
{
    Task LogChangeAsync(CreateTicketHistoryDto dto);
    Task<IEnumerable<TicketHistoryResponseDto>> GetHistoryAsync(long ticketId);
}