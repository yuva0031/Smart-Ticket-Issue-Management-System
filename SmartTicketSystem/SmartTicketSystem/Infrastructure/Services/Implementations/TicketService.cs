using AutoMapper;

using Microsoft.AspNetCore.Http;

using SmartTicketSystem.API.Events;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Events;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

/// <summary>
/// Service responsible for managing the ticket lifecycle, enforcing business rules for assignment, 
/// SLA calculations, and permission-based updates.
/// </summary>
public class TicketService : ITicketService
{
    private readonly ITicketRepository _repo;
    private readonly ITicketPriorityService _priorityService;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEventQueue _eventQueue;

    public TicketService(
        ITicketRepository repo,
        ITicketPriorityService priorityService,
        IMapper mapper,
        IEventQueue eventQueue,
        IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _priorityService = priorityService;
        _mapper = mapper;
        _eventQueue = eventQueue;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Creates a new support ticket and calculates the Due Date based on the selected priority's SLA.
    /// </summary>
    /// <param name="dto">The data transfer object containing ticket details.</param>
    /// <param name="ownerId">The unique identifier of the user creating the ticket.</param>
    /// <returns>The unique ID of the created ticket.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dto is null.</exception>
    public async Task<long> CreateAsync(CreateTicketDto dto, Guid ownerId)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var ticket = _mapper.Map<Ticket>(dto);
        ticket.OwnerId = ownerId;
        ticket.StatusId = 1; // Default status: Open

        var priority = await _priorityService.GetPriorityByIdAsync(dto.PriorityId);
        if (priority != null)
        {
            ticket.DueDate = DateTime.UtcNow.AddHours(priority.SLAHours);
        }

        await _repo.AddAsync(ticket);
        await _repo.SaveAsync();

        return ticket.TicketId;
    }

    /// <summary>
    /// Retrieves a ticket if it is visible to the requesting user based on ownership or assignment.
    /// </summary>
    public async Task<TicketResponseDto?> GetTicketVisibleToUserAsync(long ticketId, Guid userId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket == null) return null;

        bool isAuthorized = ticket.OwnerId == userId ||
                            ticket.AssignedToId == userId || 
                            HasRole("SupportManager");

        return isAuthorized ? _mapper.Map<TicketResponseDto>(ticket) : null;
    }

    /// <summary>
    /// Updates ticket details. Restricts field modifications if the user is not an Agent or Manager.
    /// </summary>
    /// <returns>True if update successful, false if unauthorized or ticket missing.</returns>
    public async Task<bool> UpdateAsync(long ticketId, UpdateTicketRequestDto dto, Guid userId)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket == null) return false;

        bool isStaff = HasRole("SupportAgent") || HasRole("SupportManager");
        if (!isStaff && ticket.OwnerId != userId) return false;

        var original = new
        {
            ticket.Description,
            ticket.PriorityId,
            ticket.StatusId,
            ticket.CategoryId,
            AssignedTo = ticket.AssignedTo?.FullName ?? "Unassigned"
        };

        if (!isStaff)
        {
            dto.StatusId = ticket.StatusId;
            dto.PriorityId = ticket.PriorityId;
            dto.AssignedToId = ticket.AssignedToId;
            dto.CategoryId = ticket.CategoryId;
        }

        _mapper.Map(dto, ticket);
        ticket.UpdatedAt = DateTime.UtcNow;

        if (dto.PriorityId.HasValue && dto.PriorityId != original.PriorityId)
        {
            var priority = await _priorityService.GetPriorityByIdAsync(dto.PriorityId.Value);
            if (priority != null)
                ticket.DueDate = ticket.CreatedAt.AddHours(priority.SLAHours);
        }

        await _repo.UpdateAsync(ticket);
        await _repo.SaveAsync();

        await PublishUpdateEvents(ticketId, userId, original, ticket);

        return true;
    }

    /// <summary>
    /// Deletes a ticket. Only owners can delete tickets that are not yet assigned.
    /// </summary>
    public async Task<bool> DeleteAsync(long ticketId, Guid userId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket == null) return false;

        if (ticket.OwnerId != userId || ticket.AssignedToId != null)
            return false;

        await _repo.DeleteAsync(ticket);
        await _repo.SaveAsync();
        return true;
    }

    public async Task<IEnumerable<TicketResponseDto>> GetAllTicketsAsync()
        => _mapper.Map<IEnumerable<TicketResponseDto>>(await _repo.GetAllTicketsAsync());

    public async Task<IEnumerable<TicketResponseDto>> GetByOwnerIdAsync(Guid ownerId)
        => _mapper.Map<IEnumerable<TicketResponseDto>>(await _repo.GetByOwnerIdAsync(ownerId));

    public async Task<IEnumerable<TicketResponseDto>> GetByAssignedToIdAsync(Guid agentId)
        => _mapper.Map<IEnumerable<TicketResponseDto>>(await _repo.GetByAssignedToAsync(agentId));

    public async Task<IEnumerable<TicketResponseDto>> GetUnassignedTicketsAsync()
        => _mapper.Map<IEnumerable<TicketResponseDto>>(await _repo.GetUnassignedAsync());

    private async Task PublishUpdateEvents(long ticketId, Guid userId, dynamic original, Ticket updated)
    {
        var changes = new Dictionary<string, string>();

        void CheckChange(string field, object? oldVal, object? newVal)
        {
            if (oldVal?.ToString() != newVal?.ToString())
                changes[field] = $"{oldVal}|{newVal}";
        }

        CheckChange("Description", original.Description, updated.Description);
        CheckChange("Priority", original.PriorityId, updated.PriorityId);
        CheckChange("Status", original.StatusId, updated.StatusId);
        CheckChange("AssignedTo", original.AssignedTo, updated.AssignedTo?.FullName ?? "Unassigned");

        if (changes.Any())
        {
            await _eventQueue.PublishAsync(new TicketUpdatedEvent(ticketId, userId, changes));
        }
    }

    private bool HasRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}