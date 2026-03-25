using InventoryManagement.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InventoryManagement.Data;

public static class AdminSeeder
{
    private const string AdminRoleName = "Admin";
    private const string DefaultAdminEmail = "admin@local.test";
    private const string DefaultAdminPassword = "Admin123!";
    private const string DefaultAdminDisplayName = "Admin";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

        var adminEmail = configuration["SeedAdmin:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = configuration["SeedAdmin:Password"] ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        var adminDisplayName = configuration["SeedAdmin:DisplayName"]
                               ?? Environment.GetEnvironmentVariable("ADMIN_DISPLAY_NAME")
                               ?? DefaultAdminDisplayName;

        if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(adminEmail) && string.IsNullOrWhiteSpace(adminPassword))
        {
            adminEmail = DefaultAdminEmail;
            adminPassword = DefaultAdminPassword;
        }

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        if (!await roleManager.RoleExistsAsync(AdminRoleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(x => x.Description)));
            }
        }

        var user = await userManager.FindByEmailAsync(adminEmail);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                DisplayName = adminDisplayName
            };

            var createResult = await userManager.CreateAsync(user, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(x => x.Description)));
            }
        }
        else
        {
            user.EmailConfirmed = true;

            if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                user.DisplayName = adminDisplayName;
            }

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", updateResult.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, AdminRoleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, AdminRoleName);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", addRoleResult.Errors.Select(x => x.Description)));
            }
        }
    }
}