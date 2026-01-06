using AutoMapper;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

/// <summary>
/// Service for managing ticket priority levels and their associated Service Level Agreements (SLA).
/// </summary>
public class TicketPriorityService : ITicketPriorityService
{
    private readonly ITicketPriorityRepository _repo;
    private readonly IMapper _mapper;

    public TicketPriorityService(
        ITicketPriorityRepository repo,
        IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a specific ticket priority entity by its unique identifier.
    /// </summary>
    public async Task<TicketPriority?> GetPriorityByIdAsync(int priorityId)
    {
        return await _repo.GetPriorityByIdAsync(priorityId);
    }

    /// <summary>
    /// Retrieves all available ticket priority levels.
    /// </summary>
    public async Task<IEnumerable<TicketPriorityDto>> GetAllAsync()
    {
        var priorities = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<TicketPriorityDto>>(priorities);
    }

    /// <summary>
    /// Updates the SLA duration for a specific priority level.
    /// </summary>
    public async Task<bool> UpdateSlaAsync(int priorityId, int slaHours)
    {
        var priority = await _repo.GetPriorityByIdAsync(priorityId);
        if (priority == null) return false;

        priority.SLAHours = slaHours;

        await _repo.UpdateAsync(priority);
        await _repo.SaveAsync();

        return true;
    }
}