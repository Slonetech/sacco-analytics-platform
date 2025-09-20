using Microsoft.AspNetCore.Identity;
using SaccoAnalytics.Core.Entities.Identity;

namespace SaccoAnalytics.Infrastructure.Services;

public class DatabaseSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public DatabaseSeeder(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new { Name = "SystemAdmin", Description = "System Administrator with full access", IsSystemRole = true },
            new { Name = "TenantAdmin", Description = "Tenant Administrator", IsSystemRole = false },
            new { Name = "FinanceUser", Description = "Finance User with reporting access", IsSystemRole = false },
            new { Name = "ViewOnly", Description = "View-only access to reports", IsSystemRole = false }
        };

        foreach (var roleInfo in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleInfo.Name))
            {
                var role = new ApplicationRole
                {
                    Name = roleInfo.Name,
                    Description = roleInfo.Description,
                    IsSystemRole = roleInfo.IsSystemRole
                };

                await _roleManager.CreateAsync(role);
            }
        }
    }
}
