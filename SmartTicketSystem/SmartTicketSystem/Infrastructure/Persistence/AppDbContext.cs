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
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketCategory> TicketCategories { get; set; }
    public DbSet<TicketPriority> TicketPriorities { get; set; }
    public DbSet<TicketStatus> TicketStatuses { get; set; }
    public DbSet<TicketHistory> TicketHistories { get; set; }
    public DbSet<TicketComment> TicketComments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>()
            .HasKey(r => r.RoleId);

        modelBuilder.Entity<UserRole>()
                    .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

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

        modelBuilder.Entity<TicketCategory>()
            .Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<TicketCategory>()
            .HasOne(c => c.AutoAssignToRole)
            .WithMany()
            .HasForeignKey(c => c.AutoAssignToRoleId)
            .OnDelete(DeleteBehavior.SetNull);

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
            .HasKey(n => n.NotificationId);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId);

        modelBuilder.Entity<RefreshToken>()
            .HasKey(rt => rt.TokenId);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId);

        modelBuilder.Entity<ErrorLog>()
            .HasKey(e => e.ErrorId);

        modelBuilder.Entity<ErrorLog>()
            .Property(e => e.Message)
            .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<ErrorLog>()
            .Property(e => e.StackTrace)
            .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "SupportManager" },
            new Role { RoleId = 3, RoleName = "SeniorAgent" },
            new Role { RoleId = 4, RoleName = "Agent" },
            new Role { RoleId = 5, RoleName = "EndUser" }
        );

        base.OnModelCreating(modelBuilder);
    }
}