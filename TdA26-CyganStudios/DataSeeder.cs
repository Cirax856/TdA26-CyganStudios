using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TdA26_CyganStudios;

public static class DataSeeder
{
    public const string DefaultUserName = "lecturer";

    public static async Task SeedDefaultUserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<int>>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();


        const string LecturerRole = "lecturer";
        if (!await roleManager.RoleExistsAsync(LecturerRole))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(LecturerRole));
        }

        string DefaultPassword = "TdA26!";
        string DefaultEmail = "lecturer@tda.com";

        var user = await userManager.FindByNameAsync(DefaultUserName);
        if (user == null)
        {
            user = new IdentityUser<int>
            {
                UserName = DefaultUserName,
                Email = DefaultEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, DefaultPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, LecturerRole);
            }
        }
    }
}
