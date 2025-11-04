using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

public class BidRepository : IBidRepository
{
    private readonly ApplicationDbContext _context;

    public BidRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Bid>> GetAllAsync()
    {
        return await _context.Bids.ToListAsync();
    }

    public async Task<Bid> GetByIdAsync(int id)
    {
        return await _context.Bids
            .Include(b => b.Auction)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Bid?> GetHighestBidForAuctionAsync(int auctionId)
    {
        return await _context.Bids
            .Where(b => b.AuctionID == auctionId)
            .OrderByDescending(b => b.Price)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Bid>> GetAllBidsWithDetailsAsync()
    {
        return await _context.Bids
            .Include(b => b.Auction)
            .Include(b => b.User)
            .OrderByDescending(b => b.BidTime)
            .ToListAsync();
    }

    public async Task<List<Bid>> GetBidsByUserIdAsync(int userId)
    {
        return await _context.Bids
            .Include(b => b.Auction)
            .Where(b => b.UserID == userId)
            .OrderByDescending(b => b.BidTime)
            .ToListAsync();
    }

    public async Task<List<Bid>> GetBidHistoryByAuctionAsync(int auctionId)
    {
        return await _context.Bids
            .Include(b => b.User)
            .Where(b => b.AuctionID == auctionId)
            .OrderByDescending(b => b.BidTime)
            .ToListAsync();
    }

    public async Task UpdateAllBidsToLosingAsync(int auctionId)
    {
        var bids = await _context.Bids
            .Where(b => b.AuctionID == auctionId && b.IsWinning)
            .ToListAsync();

        foreach (var bid in bids)
        {
            bid.IsWinning = false;
        }
    }

    public async Task AddAsync(Bid bid)
    {
        await _context.Bids.AddAsync(bid);
    }

    public async Task UpdateAsync(Bid bid)
    {
        _context.Bids.Update(bid);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}