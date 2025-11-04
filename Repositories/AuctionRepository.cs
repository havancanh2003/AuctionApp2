using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;
using MyApp.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly ApplicationDbContext _context;

    public AuctionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Auction>> GetAllAsync()
    {
        return await _context.Auctions.ToListAsync();
    }

    public async Task<IEnumerable<Auction>> GetApprovedAuctionsAsync()
    {
        return await _context.Auctions
            .Where(a => a.Status == AuctionStatus.Approved)
            .OrderByDescending(a => a.TimeStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<Auction>> GetActiveAuctionsAsync()
    {
        var now = DateTime.Now;
        return await _context.Auctions
            .Where(a => a.Status == AuctionStatus.Approved &&
                       a.TimeStart <= now &&
                       a.TimeEnd >= now)
            .OrderByDescending(a => a.TimeEnd)
            .ToListAsync();
    }

    public async Task<IEnumerable<Auction>> GetAuctionsByUserIdAsync(int userId)
    {
        return await _context.Auctions
            .Where(a => a.UserId == userId)
            .Include(a => a.Bids)
            .OrderByDescending(a => a.TimeStart)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(int auctionId, AuctionStatus status)
    {
        var auction = await _context.Auctions.FindAsync(auctionId);
        if (auction != null)
        {
            auction.Status = status;
            _context.Auctions.Update(auction);
        }
    }

    public async Task<Auction> GetByIdAsync(int id)
    {
        return await _context.Auctions.FindAsync(id);
    }

    public async Task<Auction> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Auctions
            .Include(a => a.User)
            .Include(a => a.Bids)
            .ThenInclude(b => b.User)
            .Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(Auction auction)
    {
        await _context.Auctions.AddAsync(auction);
    }

    public async Task<List<Auction>> GetAllAuctionsWithDetailsAsync()
    {
        return await _context.Auctions
            .Include(a => a.User)
            .Include(a => a.Bids)
            .ThenInclude(b => b.User)
            .Include(a => a.Payment)
            .OrderByDescending(a => a.TimeStart)
            .ToListAsync();
    }

    public async Task UpdateAsync(Auction auction)
    {
        _context.Auctions.Update(auction);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}