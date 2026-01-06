using AutoMapper;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

/// <summary>
/// Service responsible for recording and retrieving audit trails for ticket changes.
/// </summary>
public class TicketHistoryService : ITicketHistoryService
{
    private readonly ITicketHistoryRepository _repo;
    private readonly IMapper _mapper;

    public TicketHistoryService(ITicketHistoryRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    /// <summary>
    /// Logs a specific change event for a ticket (e.g., status update, assignment change).
    /// </summary>
    /// <param name="dto">The historical data to be recorded.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the DTO is null.</exception>
    public async Task LogChangeAsync(CreateTicketHistoryDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var log = _mapper.Map<TicketHistory>(dto);
        log.ChangedAt = DateTime.UtcNow;

        await _repo.AddAsync(log);
        await _repo.SaveAsync();
    }

    /// <summary>
    /// Retrieves the full audit history for a specific ticket, mapped to response DTOs.
    /// </summary>
    /// <param name="ticketId">The unique identifier of the ticket.</param>
    /// <returns>A collection of ticket history records.</returns>
    public async Task<IEnumerable<TicketHistoryResponseDto>> GetHistoryAsync(long ticketId)
    {
        var logs = await _repo.GetByTicketIdAsync(ticketId);
        return _mapper.Map<IEnumerable<TicketHistoryResponseDto>>(logs);
    }
}