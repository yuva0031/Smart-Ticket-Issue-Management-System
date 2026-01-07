using System.Security.Cryptography;
using System.Text;
using SmartTicketSystem.Domain.Entities;
using SmartTicketSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SmartTicketSystem.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Guard Clause: Check for the Admin user specifically. 
        // If the Admin exists, we assume the DB is already seeded.
        if (await context.Users.AnyAsync(u => u.Email == "yuva@gmail.com"))
        {
            return;
        }

        // 2. Seed Master Data (Constants)
        // We check .Any() for each to prevent Primary Key crashes if migrations partially ran
        if (!await context.Roles.AnyAsync()) SeedRoles(context);
        if (!await context.TicketPriorities.AnyAsync()) SeedPriorities(context);
        if (!await context.TicketStatuses.AnyAsync()) SeedStatuses(context);

        // Save Master Data immediately so IDs are available for FKs
        await context.SaveChangesAsync();

        // 3. Seed Categories
        var categoryMap = await SeedCategoriesAsync(context);
        await context.SaveChangesAsync();

        // 4. Seed Users (Admin, Managers, Agents, EndUsers)
        var admin = SeedAdmin(context);
        var managers = SeedUsersByRole(context, "Manager", 5, 2); // Role 2: SupportManager
        var agents = SeedUsersByRole(context, "Agent", 15, 3);    // Role 3: SupportAgent
        var endUsers = SeedUsersByRole(context, "User", 20, 4);   // Role 4: EndUser

        // Save Users so their Guids are committed
        await context.SaveChangesAsync();

        // 5. Seed Agent Profiles & Skills (Linked to Agents and Categories)
        var agentProfiles = SeedAgentProfiles(context, agents, categoryMap);
        await context.SaveChangesAsync();

        // 6. Seed Tickets (Linked to EndUsers, Agents, and Categories)
        SeedTickets(context, endUsers, agents, agentProfiles, categoryMap);

        // 7. Final Save to commit everything to SSMS
        await context.SaveChangesAsync();
    }

    private static User SeedAdmin(AppDbContext context)
    {
        CreatePassword("Yuva@2006", out var hash, out var salt);
        var admin = new User
        {
            FullName = "Yuva Annapareddy",
            Email = "yuva@gmail.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(admin);
        context.UserRoles.Add(new UserRole { User = admin, RoleId = 1 }); // Role 1: Admin
        return admin;
    }

    private static List<User> SeedUsersByRole(AppDbContext context, string prefix, int count, int roleId)
    {
        var users = new List<User>();
        CreatePassword("Password@123", out var hash, out var salt);

        for (int i = 1; i <= count; i++)
        {
            var user = new User
            {
                FullName = $"{prefix} {i}",
                Email = $"{prefix.ToLower()}{i}@test.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            users.Add(user);
            context.Users.Add(user);
            context.UserRoles.Add(new UserRole { User = user, RoleId = roleId });
        }
        return users;
    }

    private static async Task<Dictionary<string, int>> SeedCategoriesAsync(AppDbContext context)
    {
        var categoryData = GetCategoryDescriptions();
        var categoryMap = new Dictionary<string, int>();

        if (await context.TicketCategories.AnyAsync())
        {
            return await context.TicketCategories.ToDictionaryAsync(c => c.Name, c => c.CategoryId);
        }

        int id = 1;
        foreach (var catName in categoryData.Keys)
        {
            context.TicketCategories.Add(new TicketCategory
            {
                CategoryId = id,
                Name = catName,
                Description = categoryData[catName][0] // Use first description as default
            });
            categoryMap.Add(catName, id);
            id++;
        }
        return categoryMap;
    }

    private static List<AgentProfile> SeedAgentProfiles(AppDbContext context, List<User> agents, Dictionary<string, int> catMap)
    {
        var profiles = new List<AgentProfile>();
        var random = new Random();
        var catIds = catMap.Values.ToList();

        foreach (var agent in agents)
        {
            var profile = new AgentProfile { UserId = agent.Id, CurrentWorkload = 0 };
            profiles.Add(profile);
            context.AgentProfiles.Add(profile);

            // Assign 2 random skills to each agent
            var skills = catIds.OrderBy(x => random.Next()).Take(2);
            foreach (var catId in skills)
            {
                context.AgentCategorySkills.Add(new AgentCategorySkill { AgentProfile = profile, CategoryId = catId });
            }
        }
        return profiles;
    }

    private static void SeedTickets(AppDbContext context, List<User> users, List<User> agents, List<AgentProfile> profiles, Dictionary<string, int> catMap)
    {
        var random = new Random();
        var categoryData = GetCategoryDescriptions();
        var catNames = categoryData.Keys.ToList();

        for (int i = 1; i <= 50; i++)
        {
            string catName = catNames[random.Next(catNames.Count)];
            var descriptions = categoryData[catName];
            string description = descriptions[random.Next(descriptions.Count)];

            int statusId = random.Next(1, 6);
            var agent = agents[random.Next(agents.Count)];

            var ticket = new Ticket
            {
                Title = $"{catName} - {description}",
                Description = $"User reported an issue regarding {description}. Auto-generated seed #{i}.",
                CategoryId = catMap[catName],
                PriorityId = random.Next(1, 5), // 1 to 4 (Critical to Low)
                StatusId = statusId,
                OwnerId = users[random.Next(users.Count)].Id,
                AssignedToId = statusId > 1 ? agent.Id : null,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 10))
            };

            context.Tickets.Add(ticket);

            // Update workload for assigned agents if ticket is not closed/resolved
            if (ticket.AssignedToId.HasValue && statusId < 4)
            {
                var profile = profiles.FirstOrDefault(p => p.UserId == ticket.AssignedToId);
                if (profile != null) profile.CurrentWorkload++;
            }
        }
    }

    private static void SeedRoles(AppDbContext context)
    {
        context.Roles.AddRange(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "SupportManager" },
            new Role { RoleId = 3, RoleName = "SupportAgent" },
            new Role { RoleId = 4, RoleName = "EndUser" }
        );
    }

    private static void SeedPriorities(AppDbContext context)
    {
        context.TicketPriorities.AddRange(
            new TicketPriority { PriorityId = 1, PriorityName = "Critical", SLAHours = 4 },
            new TicketPriority { PriorityId = 2, PriorityName = "High", SLAHours = 8 },
            new TicketPriority { PriorityId = 3, PriorityName = "Medium", SLAHours = 24 },
            new TicketPriority { PriorityId = 4, PriorityName = "Low", SLAHours = 72 }
        );
    }

    private static void SeedStatuses(AppDbContext context)
    {
        context.TicketStatuses.AddRange(
            new TicketStatus { StatusId = 1, StatusName = "Created" },
            new TicketStatus { StatusId = 2, StatusName = "Assigned" },
            new TicketStatus { StatusId = 3, StatusName = "InProgress" },
            new TicketStatus { StatusId = 4, StatusName = "Resolved" },
            new TicketStatus { StatusId = 5, StatusName = "Closed" }
        );
    }

    private static Dictionary<string, List<string>> GetCategoryDescriptions()
    {
        return new Dictionary<string, List<string>>
        {
            { "Access & Accounts", new() { "account locked", "reset password", "permission denied" } },
            { "Cloud", new() { "azure outage", "aws down", "deployment error" } },
            { "Database", new() { "database connection fail", "query timeout", "deadlock" } },
            { "Hardware", new() { "laptop slow", "monitor flicker", "printer jam" } },
            { "Network", new() { "wifi disconnecting", "vpn not connecting", "dns error" } },
            { "Software", new() { "app crash", "update failed", "performance lag" } },
            { "Security", new() { "security breach", "suspicious login", "2fa failed" } }
        };
    }

    private static void CreatePassword(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
}