using Microsoft.EntityFrameworkCore;

using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<AgentProfile> AgentProfiles { get; set; }
    public DbSet<AgentCategorySkill> AgentCategorySkills { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketCategory> TicketCategories { get; set; }
    public DbSet<TicketPriority> TicketPriorities { get; set; }
    public DbSet<TicketStatus> TicketStatuses { get; set; }
    public DbSet<TicketHistory> TicketHistories { get; set; }
    public DbSet<TicketComment> TicketComments { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>()
            .HasKey(r => r.RoleId);

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithOne(u => u.UserRole)
            .HasForeignKey<UserRole>(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<AgentProfile>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<AgentProfile>()
            .HasOne(a => a.User)
            .WithMany(u => u.AgentProfiles)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<AgentCategorySkill>()
            .HasOne(s => s.AgentProfile)
            .WithMany(a => a.Skills)
            .HasForeignKey(s => s.AgentProfileId);

        modelBuilder.Entity<AgentCategorySkill>()
            .HasOne(s => s.Category)
            .WithMany()
            .HasForeignKey(s => s.CategoryId);

        modelBuilder.Entity<Ticket>()
            .HasKey(t => t.TicketId);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Owner)
            .WithMany(u => u.OwnedTickets)
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketCategory>()
            .HasKey(c => c.CategoryId);

        modelBuilder.Entity<TicketPriority>()
            .HasKey(p => p.PriorityId);

        modelBuilder.Entity<TicketStatus>()
            .HasKey(s => s.StatusId);

        modelBuilder.Entity<TicketComment>()
            .HasKey(tc => tc.CommentId);

        modelBuilder.Entity<TicketComment>()
            .HasOne(tc => tc.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(tc => tc.TicketId);

        modelBuilder.Entity<TicketComment>()
            .HasOne(tc => tc.User)
            .WithMany(u => u.TicketComments)
            .HasForeignKey(tc => tc.UserId);

        modelBuilder.Entity<TicketHistory>()
            .HasKey(th => th.HistoryId);

        modelBuilder.Entity<TicketHistory>()
            .HasOne(th => th.Ticket)
            .WithMany(t => t.Histories)
            .HasForeignKey(th => th.TicketId);

        modelBuilder.Entity<TicketHistory>()
            .HasOne(th => th.User)
            .WithMany(u => u.TicketHistories)
            .HasForeignKey(th => th.ModifiedBy)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Notification>()
            .HasKey(n => n.Id);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId);

        // Configure ValueGeneratedNever for manual ID assignment
        modelBuilder.Entity<Role>()
            .Property(r => r.RoleId)
            .ValueGeneratedNever();

        modelBuilder.Entity<TicketStatus>()
            .Property(s => s.StatusId)
            .ValueGeneratedNever();

        modelBuilder.Entity<TicketPriority>()
            .Property(p => p.PriorityId)
            .ValueGeneratedNever();

        modelBuilder.Entity<TicketCategory>()
            .Property(c => c.CategoryId)
            .ValueGeneratedNever();
    }
}