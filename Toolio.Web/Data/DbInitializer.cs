using Microsoft.AspNetCore.Identity;

namespace Toolio.Web.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Create Admin role if it does not exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(
                new IdentityRole("Admin")
            );
        }

        // Development admin account
        string adminEmail = "admin@toolio.com";
        string adminPassword = "Admin@12345";

        var adminUser =
            await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result =
                await userManager.CreateAsync(
                    adminUser,
                    adminPassword
                );

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    adminUser,
                    "Admin"
                );
            }
        }
        else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(
                adminUser,
                "Admin"
            );
        }
    }
}