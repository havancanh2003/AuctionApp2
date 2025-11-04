using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Dtos;
using MyApp.Models;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Users
        await SeedUsersAsync(userManager);

        // Seed other data
        await SeedAuctionDataAsync(context, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        string[] roleNames = { "Admin", "Customer", "Seller" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager)
    {
        // Admin User
        var adminUser = new User
        {
            UserName = "admin@auction.com",
            Email = "admin@auction.com",
            Name = "Admin User",
            Role = (int)RoleTypeEnum.Admin,
            EmailConfirmed = true,
            IsActived = true,
            PhoneNumber = "0123456789",
            Address = "123 Đường Lê Lợi, Quận 1, TP.HCM",
            CreatedAt = DateTime.Now
        };

        if (await userManager.FindByEmailAsync(adminUser.Email) == null)
        {
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seller Users
        var seller1 = new User
        {
            UserName = "seller1@auction.com",
            Email = "seller1@auction.com",
            Name = "Nguyễn Văn Bán",
            Role = (int)RoleTypeEnum.Seller,
            EmailConfirmed = true,
            IsActived = true,
            PhoneNumber = "0987654321",
            Address = "456 Đường Nguyễn Huệ, Quận 1, TP.HCM",
            CreatedAt = DateTime.Now
        };

        if (await userManager.FindByEmailAsync(seller1.Email) == null)
        {
            var result = await userManager.CreateAsync(seller1, "Seller123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(seller1, "Seller");
            }
        }

        var seller2 = new User
        {
            UserName = "seller2@auction.com",
            Email = "seller2@auction.com",
            Name = "Trần Thị Bán Hàng",
            Role = (int)RoleTypeEnum.Seller,
            EmailConfirmed = true,
            IsActived = true,
            PhoneNumber = "0912345678",
            Address = "789 Đường Pasteur, Quận 3, TP.HCM",
            CreatedAt = DateTime.Now
        };

        if (await userManager.FindByEmailAsync(seller2.Email) == null)
        {
            var result = await userManager.CreateAsync(seller2, "Seller123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(seller2, "Seller");
            }
        }

        // Customer Users
        var customers = new[]
        {
            new { Email = "customer1@auction.com", Name = "Phạm Văn Mua", Phone = "0934567890" },
            new { Email = "customer2@auction.com", Name = "Lê Thị Mua Hàng", Phone = "0945678901" },
            new { Email = "customer3@auction.com", Name = "Hoàng Văn Đấu Giá", Phone = "0956789012" }
        };

        foreach (var customer in customers)
        {
            var user = new User
            {
                UserName = customer.Email,
                Email = customer.Email,
                Name = customer.Name,
                Role = (int)RoleTypeEnum.Customer,
                EmailConfirmed = true,
                IsActived = true,
                PhoneNumber = customer.Phone,
                Address = "TP.HCM",
                CreatedAt = DateTime.Now
            };

            if (await userManager.FindByEmailAsync(user.Email) == null)
            {
                var result = await userManager.CreateAsync(user, "Customer123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer");
                }
            }
        }
    }

    private static async Task SeedAuctionDataAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        if (!context.Auctions.Any())
        {
            var seller1 = await userManager.FindByEmailAsync("seller1@auction.com");
            var seller2 = await userManager.FindByEmailAsync("seller2@auction.com");

            var auctions = new[]
            {
                new Auction
                {
                    UserId = seller1.Id,
                    Name = "iPhone 15 Pro Max 256GB",
                    Description = "iPhone 15 Pro Max màu Titan tự nhiên, mới 100%, full box, bảo hành Apple 12 tháng.",
                    ImgUrl = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    PriceStart = 25000000,
                    TimeStart = DateTime.Now.AddDays(-2),
                    TimeEnd = DateTime.Now.AddDays(5),
                    Status = AuctionStatus.Approved
                },
                new Auction
                {
                    UserId = seller1.Id,
                    Name = "MacBook Air M2 2023",
                    Description = "MacBook Air M2 13 inch, RAM 8GB, SSD 256GB, màu Midnight.",
                    ImgUrl = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    PriceStart = 28000000,
                    TimeStart = DateTime.Now.AddDays(-1),
                    TimeEnd = DateTime.Now.AddDays(6),
                    Status = AuctionStatus.Approved
                },
                new Auction
                {
                    UserId = seller2.Id,
                    Name = "Samsung Galaxy S24 Ultra",
                    Description = "Samsung Galaxy S24 Ultra 512GB, màu đen, mới seal.",
                    ImgUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    PriceStart = 22000000,
                    TimeStart = DateTime.Now.AddDays(-3),
                    TimeEnd = DateTime.Now.AddDays(3),
                    Status = AuctionStatus.Approved
                }
            };

            await context.Auctions.AddRangeAsync(auctions);
            await context.SaveChangesAsync();
        }
    }
}