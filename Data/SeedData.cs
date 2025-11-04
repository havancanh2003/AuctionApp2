using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // === 1. Đảm bảo DB đã tạo ===
            await context.Database.MigrateAsync();

            // === 2. Tạo các vai trò mặc định (Admin, Customer, Seller) ===
            foreach (RoleTypeEnum role in Enum.GetValues(typeof(RoleTypeEnum)))
            {
                string roleName = role.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            // === 3. Tạo tài khoản mặc định cho từng vai trò ===
            await CreateUserIfNotExists(userManager, "admin@system.com", "123456", RoleTypeEnum.Admin);
            await CreateUserIfNotExists(userManager, "customer@system.com", "123456", RoleTypeEnum.Customer);
            await CreateUserIfNotExists(userManager, "seller@system.com", "123456", RoleTypeEnum.Seller);
        }

        private static async Task CreateUserIfNotExists(
            UserManager<User> userManager,
            string email,
            string password,
            RoleTypeEnum role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = email,
                    Email = email,
                    Name = role.ToString(),
                    Role = (int)role,
                    EmailConfirmed = true,
                    IsActived = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role.ToString());
                    Console.WriteLine($"Created {role} account: {email}");
                }
                else
                {
                    Console.WriteLine($"Failed to create {role} account: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
