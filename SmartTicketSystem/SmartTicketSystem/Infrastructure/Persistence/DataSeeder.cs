using Microsoft.EntityFrameworkCore;

using SmartTicketSystem.Domain.Entities;

namespace SmartTicketSystem.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "SupportManager" },
                new Role { RoleId = 3, RoleName = "SeniorAgent" },
                new Role { RoleId = 4, RoleName = "Agent" },
                new Role { RoleId = 5, RoleName = "EndUser" }
            );

            await context.SaveChangesAsync();
        }
    }
}