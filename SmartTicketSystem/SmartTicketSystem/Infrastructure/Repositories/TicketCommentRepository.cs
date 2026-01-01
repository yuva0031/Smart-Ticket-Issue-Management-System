using Microsoft.EntityFrameworkCore;

using SmartTicketSystem.Application.Interfaces.Repositories;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Persistence;

namespace SmartTicketSystem.Infrastructure.Repositories;

public class TicketCommentRepository : ITicketCommentRepository
{
    private readonly AppDbContext _context;
    public TicketCommentRepository(AppDbContext context) => _context = context;

    public async Task AddCommentAsync(TicketComment comment)
        => await _context.TicketComments.AddAsync(comment);

    public async Task<IEnumerable<TicketComment>> GetCommentsByTicketAsync(long ticketId)
        => await _context.TicketComments
            .Include(c => c.User)
            .Where(c => c.TicketId == ticketId)
            .OrderByDescending(c => c.CommentId)
            .ToListAsync();

    public async Task<TicketComment?> GetByIdAsync(long commentId)
        => await _context.TicketComments.FirstOrDefaultAsync(c => c.CommentId == commentId);

    public async Task UpdateCommentAsync(TicketComment comment)
    {
        _context.TicketComments.Update(comment);
        await _context.SaveChangesAsync();
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();
}