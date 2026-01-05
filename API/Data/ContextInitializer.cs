using API.Models;
using API.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public static class ContextInitializer
    {
        public static async Task InitializeAsync(Context context, UserManager<AppUser> userManager)
        {
            if (context.Database.GetPendingMigrations().Count() > 0)
            {
                await context.Database.MigrateAsync();
            }

            // seeding dummy user
            if (!userManager.Users.Any())
            {
                var john = new AppUser { Name = "john", UserName = "john", Email = "john@example.com", EmailConfirmed = true, LockoutEnabled = true };

                await userManager.CreateAsync(john, SD.DefaultPassword);

                var peter = new AppUser { Name = "peter", UserName = "peter", Email = "peter@example.com", EmailConfirmed = true, LockoutEnabled = true };

                await userManager.CreateAsync(peter, SD.DefaultPassword);

                var maddie = new AppUser { Name = "maddie", UserName = "maddie", Email = "maddie@example.com", EmailConfirmed = true, LockoutEnabled = true };

                await userManager.CreateAsync(maddie, SD.DefaultPassword);

                var lisa = new AppUser { Name = "lisa", UserName = "lisa", Email = "lisa@example.com", EmailConfirmed = true, LockoutEnabled = true };

                await userManager.CreateAsync(lisa, SD.DefaultPassword);

                var brady = new AppUser { Name = "brady", UserName = "brady", Email = "brady@example.com", EmailConfirmed = true, LockoutEnabled = true };

                await userManager.CreateAsync(brady, SD.DefaultPassword);
            }
        }
    }
}
