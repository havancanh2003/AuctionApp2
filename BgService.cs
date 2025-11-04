using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyApp.Models;
using MyApp.Repositories;
namespace MyApp
{
   public class AuctionMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AuctionMonitorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var auctionRepo = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
                var bidRepo = scope.ServiceProvider.GetRequiredService<IBidRepository>();
                var paymentRepo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

                var expiredAuctions = auctionRepo.GetAll()
                    .Where(a => a.Status == AuctionStatus.Approved && a.TimeEnd <= DateTime.Now)
                    .ToList();

                foreach (var auction in expiredAuctions)
                {
                    var highestBid = bidRepo.GetHighestBidForAuction(auction.Id);
                    if (highestBid != null)
                    {
                        var payment = new Payment
                        {
                            AuctionID = auction.Id,
                            UserID = highestBid.UserID,
                            Price = highestBid.Price,
                            CreatedAt = DateTime.Now
                        };
                        paymentRepo.Add(payment);
                    }

                    auction.Status = AuctionStatus.Closed;
                    auctionRepo.Update(auction);
                }

                auctionRepo.Save();
                paymentRepo.Save();

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // kiểm tra mỗi phút
            }
        }
    }
}