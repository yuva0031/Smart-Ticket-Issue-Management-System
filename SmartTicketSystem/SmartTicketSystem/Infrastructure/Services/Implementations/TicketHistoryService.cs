using AutoMapper;

using Microsoft.AspNetCore.SignalR;

using SmartTicketSystem.API.Hubs;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

public class TicketHistoryService : ITicketHistoryService
{
    private readonly ITicketHistoryRepository _repo;
    private readonly IMapper _mapper;
    public TicketHistoryService(ITicketHistoryRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task LogChangeAsync(CreateTicketHistoryDto dto)
    {
        var log = _mapper.Map<TicketHistory>(dto);
        await _repo.AddAsync(log);
        await _repo.SaveAsync();
    }

    public async Task<IEnumerable<TicketHistoryResponseDto>> GetHistoryAsync(long ticketId)
    {
        var logs = await _repo.GetByTicketIdAsync(ticketId);
        return _mapper.Map<IEnumerable<TicketHistoryResponseDto>>(logs);
    }
}