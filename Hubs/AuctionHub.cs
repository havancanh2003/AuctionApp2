using Microsoft.AspNetCore.SignalR;

public class AuctionHub : Hub
{
    public async Task JoinAuctionGroup(int auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());
    }

    public async Task LeaveAuctionGroup(int auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
    }

    public async Task SendBid(int auctionId, decimal price, string userName)
    {
        await Clients.Group(auctionId.ToString()).SendAsync("ReceiveBid", new
        {
            auctionId,
            price,
            userName,
            time = DateTime.Now.ToString("HH:mm:ss")
        });
    }
}