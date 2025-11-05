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

        // Seed Auctions
        await SeedAuctionDataAsync(context, userManager);

        // Seed Bids với BidCode
        await SeedBidDataAsync(context, userManager);

        // Seed Payments
        await SeedPaymentDataAsync(context, userManager);
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
            // Seller 1 Auctions
            new Auction
            {
                UserId = seller1.Id,
                Name = "iPhone 15 Pro Max 256GB",
                Description = "iPhone 15 Pro Max màu Titan tự nhiên, mới 100%, full box, bảo hành Apple 12 tháng. Sản phẩm chính hãng, đầy đủ phụ kiện.",
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
                Description = "MacBook Air M2 13 inch, RAM 8GB, SSD 256GB, màu Midnight. Máy mới mua 2 tháng, còn bảo hành 10 tháng.",
                ImgUrl = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                PriceStart = 28000000,
                TimeStart = DateTime.Now.AddDays(-1),
                TimeEnd = DateTime.Now.AddDays(6),
                Status = AuctionStatus.Approved
            },
            new Auction
            {
                UserId = seller1.Id,
                Name = "AirPods Pro 2",
                Description = "AirPods Pro 2 thế hệ mới nhất, chống ồn chủ động, mới mua 1 tuần. Lý do bán: không hợp với tai.",
                ImgUrl = "https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                PriceStart = 5000000,
                TimeStart = DateTime.Now.AddDays(1),
                TimeEnd = DateTime.Now.AddDays(8),
                Status = AuctionStatus.Pending
            },

            // Seller 2 Auctions
            new Auction
            {
                UserId = seller2.Id,
                Name = "Samsung Galaxy S24 Ultra",
                Description = "Samsung Galaxy S24 Ultra 512GB, màu đen, mới seal. Sản phẩm nhập khẩu chính hãng, có hóa đơn VAT.",
                ImgUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                PriceStart = 22000000,
                TimeStart = DateTime.Now.AddDays(-3),
                TimeEnd = DateTime.Now.AddDays(3),
                Status = AuctionStatus.Approved
            },
            new Auction
            {
                UserId = seller2.Id,
                Name = "Apple Watch Series 9",
                Description = "Apple Watch Series 9 45mm, dây thể thao, màu Starlight. Đồng hồ mới 99%, ít sử dụng, còn bảo hành 8 tháng.",
                ImgUrl = "https://images.unsplash.com/photo-1633463366066-4dc0c4c6d6e6?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                PriceStart = 12000000,
                TimeStart = DateTime.Now,
                TimeEnd = DateTime.Now.AddDays(7),
                Status = AuctionStatus.Approved
            },
            new Auction
            {
                UserId = seller2.Id,
                Name = "iPad Pro 12.9 inch M1",
                Description = "iPad Pro 12.9 inch 2021, chip M1, RAM 8GB, SSD 128GB. Máy đẹp như mới, bao test thoải mái.",
                ImgUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                PriceStart = 18000000,
                TimeStart = DateTime.Now.AddDays(-10),
                TimeEnd = DateTime.Now.AddDays(-1),
                Status = AuctionStatus.Closed
            }
        };

            await context.Auctions.AddRangeAsync(auctions);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedBidDataAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        if (!context.Bids.Any())
        {
            var customer1 = await userManager.FindByEmailAsync("customer1@auction.com");
            var customer2 = await userManager.FindByEmailAsync("customer2@auction.com");
            var customer3 = await userManager.FindByEmailAsync("customer3@auction.com");

            var auctions = await context.Auctions.ToListAsync();

            var bids = new[]
            {
            // Bids for iPhone 15 (Auction 1)
            new Bid
            {
                AuctionID = auctions[0].Id,
                UserID = customer1.Id,
                Price = 25500000,
                BidTime = DateTime.Now.AddDays(-1).AddHours(-2),
                IsWinning = false,
                BidCode = "BD01A202"
            },
            new Bid
            {
                AuctionID = auctions[0].Id,
                UserID = customer2.Id,
                Price = 26000000,
                BidTime = DateTime.Now.AddDays(-1).AddHours(-1),
                IsWinning = false,
                BidCode = "BD02B303"
            },
            new Bid
            {
                AuctionID = auctions[0].Id,
                UserID = customer3.Id,
                Price = 26500000,
                BidTime = DateTime.Now.AddDays(-1),
                IsWinning = true,
                BidCode = "BD03C404"
            },

            // Bids for MacBook Air (Auction 2)
            new Bid
            {
                AuctionID = auctions[1].Id,
                UserID = customer2.Id,
                Price = 28500000,
                BidTime = DateTime.Now.AddHours(-18),
                IsWinning = false,
                BidCode = "BD04D505"
            },
            new Bid
            {
                AuctionID = auctions[1].Id,
                UserID = customer1.Id,
                Price = 29000000,
                BidTime = DateTime.Now.AddHours(-12),
                IsWinning = true,
                BidCode = "BD05E606"
            },

            // Bids for Samsung S24 (Auction 3)
            new Bid
            {
                AuctionID = auctions[3].Id,
                UserID = customer3.Id,
                Price = 22500000,
                BidTime = DateTime.Now.AddDays(-2),
                IsWinning = false,
                BidCode = "BD06F707"
            },
            new Bid
            {
                AuctionID = auctions[3].Id,
                UserID = customer1.Id,
                Price = 23000000,
                BidTime = DateTime.Now.AddDays(-1),
                IsWinning = false,
                BidCode = "BD07G808"
            },
            new Bid
            {
                AuctionID = auctions[3].Id,
                UserID = customer2.Id,
                Price = 23500000,
                BidTime = DateTime.Now.AddHours(-12),
                IsWinning = true,
                BidCode = "BD08H909"
            },

            // Bids for Apple Watch (Auction 4)
            new Bid
            {
                AuctionID = auctions[4].Id,
                UserID = customer1.Id,
                Price = 12500000,
                BidTime = DateTime.Now.AddHours(-2),
                IsWinning = true,
                BidCode = "BD09I010"
            },
            new Bid
            {
                AuctionID = auctions[4].Id,
                UserID = customer2.Id,
                Price = 12300000,
                BidTime = DateTime.Now.AddHours(-3),
                IsWinning = false,
                BidCode = "BD10J111"
            },

            // Bids for iPad Pro (Auction 5 - Closed)
            new Bid
            {
                AuctionID = auctions[5].Id,
                UserID = customer1.Id,
                Price = 18500000,
                BidTime = DateTime.Now.AddDays(-5),
                IsWinning = false,
                BidCode = "BD11K212"
            },
            new Bid
            {
                AuctionID = auctions[5].Id,
                UserID = customer2.Id,
                Price = 19000000,
                BidTime = DateTime.Now.AddDays(-4),
                IsWinning = false,
                BidCode = "BD12L313"
            },
            new Bid
            {
                AuctionID = auctions[5].Id,
                UserID = customer3.Id,
                Price = 19500000,
                BidTime = DateTime.Now.AddDays(-3),
                IsWinning = true,
                BidCode = "BD13M414"
            }
        };

            await context.Bids.AddRangeAsync(bids);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedPaymentDataAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        if (!context.Payments.Any())
        {
            var customer3 = await userManager.FindByEmailAsync("customer3@auction.com");
            var customer2 = await userManager.FindByEmailAsync("customer2@auction.com");
            var customer1 = await userManager.FindByEmailAsync("customer1@auction.com");

            var auctions = await context.Auctions.ToListAsync();

            var payments = new[]
            {
            // Payment for iPad Pro (Closed auction - customer3 won)
            new Payment
            {
                UserID = customer3.Id,
                AuctionID = auctions[5].Id, // iPad Pro
                Price = 19500000,
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now.AddDays(-1),
                Status = PaymentStatus.Complete
            },
            // Payment for Samsung S24 (customer2 won - unpaid)
            new Payment
            {
                UserID = customer2.Id,
                AuctionID = auctions[3].Id, // Samsung S24
                Price = 23500000,
                CreatedAt = DateTime.Now.AddHours(-2),
                UpdatedAt = DateTime.Now.AddHours(-2),
                Status = PaymentStatus.Unpaid
            },
            // Payment for Apple Watch (customer1 won - in transit)
            new Payment
            {
                UserID = customer1.Id,
                AuctionID = auctions[4].Id, // Apple Watch
                Price = 12500000,
                CreatedAt = DateTime.Now.AddHours(-1),
                UpdatedAt = DateTime.Now.AddMinutes(-30),
                Status = PaymentStatus.InTransit
            },
            // Payment for iPhone 15 (customer3 won - unpaid)
            new Payment
            {
                UserID = customer3.Id,
                AuctionID = auctions[0].Id, // iPhone 15
                Price = 26500000,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = PaymentStatus.Unpaid
            },
            // Payment for MacBook Air (customer1 won - unpaid)
            new Payment
            {
                UserID = customer1.Id,
                AuctionID = auctions[1].Id, // MacBook Air
                Price = 29000000,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Status = PaymentStatus.Unpaid
            }
        };

            await context.Payments.AddRangeAsync(payments);
            await context.SaveChangesAsync();
        }
    }
}