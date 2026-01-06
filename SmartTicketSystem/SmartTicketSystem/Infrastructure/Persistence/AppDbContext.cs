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

        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "SupportManager" },
            new Role { RoleId = 3, RoleName = "SupportAgent" },
            new Role { RoleId = 4, RoleName = "EndUser" }
        );

        modelBuilder.Entity<TicketCategory>().HasData(

            new TicketCategory
            {
                CategoryId = 1,
                Name = "Network",
                Description = "Network, Wi-Fi, VPN, DNS, or connectivity issues"
            },

            new TicketCategory
            {
                CategoryId = 2,
                Name = "Hardware",
                Description = "Physical device or peripheral related issues"
            },

            new TicketCategory
            {
                CategoryId = 3,
                Name = "Software",
                Description = "Application, OS, or software malfunction"
            },

            new TicketCategory
            {
                CategoryId = 4,
                Name = "Cloud",
                Description = "Cloud platforms, deployments, or infrastructure issues"
            },

            new TicketCategory
            {
                CategoryId = 5,
                Name = "Security",
                Description = "Security alerts, access blocks, or threats"
            },

            new TicketCategory
            {
                CategoryId = 6,
                Name = "HR",
                Description = "HR systems, payroll, or employee services"
            },

            new TicketCategory
            {
                CategoryId = 7,
                Name = "Finance",
                Description = "Billing, payments, or financial transactions"
            },

            new TicketCategory
            {
                CategoryId = 8,
                Name = "Email & Communication",
                Description = "Email or communication service issues"
            },

            new TicketCategory
            {
                CategoryId = 9,
                Name = "Access & Accounts",
                Description = "Account, role, or permission related issues"
            },

            new TicketCategory
            {
                CategoryId = 10,
                Name = "Database",
                Description = "Database connectivity or data issues"
            },

            new TicketCategory
            {
                CategoryId = 11,
                Name = "DevOps & Deployment",
                Description = "CI/CD, build, or deployment failures"
            },

            new TicketCategory
            {
                CategoryId = 12,
                Name = "Miscellaneous",
                Description = "Uncategorized or general issues"
            }
        );



        modelBuilder.Entity<TicketPriority>().HasData(
            new TicketPriority { PriorityId = 1, PriorityName = "Critical", SLAHours = 4 },
            new TicketPriority { PriorityId = 2, PriorityName = "High", SLAHours = 8 },
            new TicketPriority { PriorityId = 3, PriorityName = "Medium", SLAHours = 24 },
            new TicketPriority { PriorityId = 4, PriorityName = "Low", SLAHours = 72 }
        );


        modelBuilder.Entity<TicketStatus>().HasData(
            new TicketStatus { StatusId = 1, StatusName = "Created" },
            new TicketStatus { StatusId = 2, StatusName = "Assigned" },
            new TicketStatus { StatusId = 3, StatusName = "InProgress" },
            new TicketStatus { StatusId = 4, StatusName = "Resolved" },
            new TicketStatus { StatusId = 5, StatusName = "Closed" },
            new TicketStatus { StatusId = 6, StatusName = "Reopened" },
            new TicketStatus { StatusId = 7, StatusName = "Cancelled" }
        );

        base.OnModelCreating(modelBuilder);
    }
}