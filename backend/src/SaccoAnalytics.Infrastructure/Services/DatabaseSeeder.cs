using Microsoft.AspNetCore.Identity;
using SaccoAnalytics.Core.Entities.Identity;

namespace SaccoAnalytics.Infrastructure.Services;

public class DatabaseSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DatabaseSeeder(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedSystemAdminAsync();
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

    private async Task SeedSystemAdminAsync()
    {
        var adminEmail = "admin@saccoanalytics.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "SystemAdmin");
            }
        }
    }
}
