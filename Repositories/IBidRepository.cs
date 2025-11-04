using Microsoft.EntityFrameworkCore;
using MyApp.Models;

public interface IBidRepository
{
    Task<IEnumerable<Bid>> GetAllAsync();
    Task<Bid> GetByIdAsync(int id);
    Task<Bid?> GetHighestBidForAuctionAsync(int auctionId);
    Task<List<Bid>> GetAllBidsWithDetailsAsync();
    Task<List<Bid>> GetBidsByUserIdAsync(int userId);
    Task<List<Bid>> GetBidHistoryByAuctionAsync(int auctionId);
    Task AddAsync(Bid bid);
    Task UpdateAsync(Bid bid);
    Task UpdateAllBidsToLosingAsync(int auctionId);
    Task SaveAsync();
}