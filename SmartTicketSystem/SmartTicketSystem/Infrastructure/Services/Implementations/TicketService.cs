using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using SmartTicketSystem.API.Events;
using SmartTicketSystem.API.Hubs;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Domain.Enums;
using SmartTicketSystem.Infrastructure.Events;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repo;
    private readonly ITicketHistoryService _historyService;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEventQueue _eventQueue;
    private readonly IHubContext<TicketHub> _hub;

    public TicketService(ITicketRepository repo, ITicketHistoryService historyService, IMapper mapper, IHubContext<TicketHub> hub, IEventQueue eventQueue, IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _mapper = mapper;
        _historyService = historyService;
        _httpContextAccessor = httpContextAccessor;
        _hub = hub;
        _eventQueue = eventQueue;
    }

    public async Task<long> CreateAsync(CreateTicketDto dto, Guid ownerId)
    {
        var ticket = _mapper.Map<Ticket>(dto);
        ticket.OwnerId = ownerId; 
        ticket.StatusId = 1;

        await _repo.AddAsync(ticket);
        await _repo.SaveAsync();

        return ticket.TicketId;
    }

    public async Task<TicketResponseDto?> GetTicketVisibleToUserAsync(long ticketId, Guid userId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket == null) return null;

        bool allowed =
           ticket.OwnerId == userId ||          
           ticket.AssignedToId == userId ||       
           ticket.AssignedToId == null ||
           true;                                 

        return allowed ? _mapper.Map<TicketResponseDto>(ticket) : null;
    }

    public async Task<IEnumerable<TicketResponseDto>> GetByOwnerIdAsync(Guid ownerId)
        => _mapper.Map<IEnumerable<TicketResponseDto>>(await _repo.GetByOwnerIdAsync(ownerId));

    public async Task<IEnumerable<TicketResponseDto>> GetByAssignedToIdAsync(Guid agentId)
        => _mapper.Map<IEnumerable<TicketResponseDto>>(await _repo.GetByAssignedToAsync(agentId));

    public async Task<IEnumerable<TicketResponseDto>> GetUnassignedTicketsAsync()
        => _mapper.Map<IEnumerable<TicketResponseDto>>(await _repo.GetUnassignedAsync());

    public async Task<bool> UpdateAsync(long ticketId, UpdateTicketDto dto, Guid userId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket == null) return false;

        if (!(ticket.OwnerId == userId || ticket.AssignedToId == userId || HasRole("SupportManager")))
            return false;

        var original = new
        {
            ticket.Title,
            ticket.Description,
            PriorityId = ticket.PriorityId,
            StatusId = ticket.StatusId,
            ticket.AssignedToId
        };
        Console.WriteLine("updatable: "+original.StatusId);

        _mapper.Map(dto, ticket);
        await _repo.UpdateAsync(ticket);
        await _repo.SaveAsync();

        var changes = new Dictionary<string, string>();

        void TrackChange(string field, object oldVal, object newVal)
        {
            var oldValue = oldVal?.ToString() ?? "";
            var newValue = newVal?.ToString() ?? "";
            if (oldValue == newValue) return;
            changes[field] = $"{oldValue}|{newValue}";
        }

        TrackChange("Title", original.Title, ticket.Title);
        TrackChange("Description", original.Description, ticket.Description);
        TrackChange("Priority", original.PriorityId, ticket.PriorityId);
        TrackChange("Status", original.StatusId, ticket.StatusId);
        TrackChange("AssignedTo", original.AssignedToId, ticket.AssignedToId);


        await _eventQueue.PublishAsync(
            new TicketUpdatedEvent(ticketId, userId, changes)
        );

        return true;
    }

    private bool HasRole(string role)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user != null && user.IsInRole(role);
    }


    public async Task<bool> DeleteAsync(long ticketId, Guid userId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket == null) return false;

        if (ticket.OwnerId != userId && ticket.AssignedToId != null)
            return false;

        await _repo.DeleteAsync(ticket);
        await _repo.SaveAsync();
        return true;
    }
}