using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MyApp.Hubs
{
    public class AuctionHub : Hub
    {
        public async Task SendHighestBid(int auctionId, decimal price)
        {
            await Clients.All.SendAsync("ReceiveHighestBid", auctionId, price);
        }
    }
}