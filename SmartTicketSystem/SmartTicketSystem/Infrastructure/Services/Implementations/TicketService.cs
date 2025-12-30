using System;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using SmartTicketSystem.API.Controllers;
using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Application.Services.Interfaces;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Domain.Enums;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Services.Implementations;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repo;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public TicketService(ITicketRepository repo, IMapper mapper, AppDbContext context)
    {
        _repo = repo;
        _mapper = mapper;
        _context = context;
    }

    public async Task<long> CreateAsync(CreateTicketDto dto, Guid ownerId)
    {
        var priority = await _context.TicketPriorities.FindAsync(dto.PriorityId);

        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            OwnerId = ownerId,
            PriorityId = dto.PriorityId,
            StatusId = 1,
            DueDate = DateTime.UtcNow.AddHours(priority.SLAHours)
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket.TicketId;
    }

    public async Task<TicketResponseDto> GetByIdAsync(long ticketId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        return ticket == null ? null : _mapper.Map<TicketResponseDto>(ticket);
    }

    public async Task<IEnumerable<TicketResponseDto>> GetAllAsync()
    {
        var data = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<TicketResponseDto>>(data);
    }

    public async Task<IEnumerable<TicketResponseDto>> GetByOwnerIdAsync(Guid ownerId)
    {
        var data = await _repo.GetByOwnerIdAsync(ownerId);
        return _mapper.Map<IEnumerable<TicketResponseDto>>(data);
    }

    public async Task<IEnumerable<TicketResponseDto>> GetByAssignedToIdAsync(Guid assignedToId)
    {
        var data = await _repo.GetByAssignedToIdAsync(assignedToId);
        return _mapper.Map<IEnumerable<TicketResponseDto>>(data);
    }

    public async Task<bool> UpdateAsync(long ticketId, UpdateTicketDto dto, Guid modifiedBy)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket == null) return false;

        if (dto.PriorityId.HasValue) ticket.PriorityId = dto.PriorityId.Value;
        if (dto.AssignedToId.HasValue) ticket.AssignedToId = dto.AssignedToId.Value;
        ticket.StatusId = dto.StatusId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(ticket);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long ticketId)
    {
        var ticket = await _repo.GetByIdAsync(ticketId);
        if (ticket == null) return false;

        ticket.IsDeleted = true;
        await _repo.SaveChangesAsync();
        return true;
    }
}