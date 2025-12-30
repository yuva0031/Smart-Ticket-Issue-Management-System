using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Services;

namespace SmartTicketSystem.Infrastructure.Persistence;

public class AutoAssignmentWorker : BackgroundService
{
    private readonly IServiceProvider _provider;

    public AutoAssignmentWorker(IServiceProvider provider)
    {
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var lucene = scope.ServiceProvider.GetRequiredService<LuceneCategoryMatcher>();

            var unassignedTickets = await context.Tickets
                .Where(t => t.AssignedToId == null && t.StatusId == 1)
                .ToListAsync(stoppingToken);

            foreach (var ticket in unassignedTickets)
            {
                int detectedCategoryId = lucene.DetectCategory(ticket.Description);
                if (detectedCategoryId > 0 && ticket.CategoryId is null)
                {
                    ticket.CategoryId = detectedCategoryId;
                }

                var bestAgent = await context.AgentProfiles
                    .Include(a => a.Skills)
                    .Where(a => a.Skills.Any(s => s.CategoryId == ticket.CategoryId))
                    .OrderBy(a => a.CurrentWorkload)
                    .FirstOrDefaultAsync(stoppingToken);

                if (bestAgent != null)
                {
                    ticket.AssignedToId = bestAgent.UserId;
                    ticket.StatusId = 2;
                    bestAgent.CurrentWorkload += 1;

                    await context.TicketHistories.AddAsync(new TicketHistory
                    {
                        TicketId = ticket.TicketId,
                        ModifiedBy = bestAgent.UserId,
                        FieldName = "AssignedTo",
                        OldValue = "Unassigned",
                        NewValue = bestAgent.UserId.ToString(),
                        ChangedAt = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}