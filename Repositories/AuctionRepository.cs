using MyApp.Models;
using MyApp.Data;
using Microsoft.EntityFrameworkCore;
namespace MyApp.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly ApplicationDbContext _context;

        public AuctionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Auction> GetAll()
        {
            return _context.Auctions.ToList();
        }
        public IEnumerable<Auction> GetApprovedAuctions()
        {
            return _context.Auctions.Where(a => a.Status == AuctionStatus.Approved).ToList();
        }
        public IEnumerable<Auction> GetActiveAuctions()
        {
            var now = DateTime.Now;
            return _context.Auctions.Where(a => a.Status == AuctionStatus.Approved && a.TimeStart <= now && a.TimeEnd >= now).ToList();
        }
        public void UpdateStatus(int auctionId, AuctionStatus status)
        {
            var auction = _context.Auctions.Find(auctionId);
            if (auction != null)
            {
                auction.Status = status;
                _context.Auctions.Update(auction);
            }
        }
        public Auction GetById(int id)
        {
            return _context.Auctions.Find(id);
        }

        public void Add(Auction auction)
        {
            _context.Auctions.Add(auction);
        }
        public List<Auction> GetAllAuctionsWithDetails()
        {
            return _context.Auctions
                .Include(a => a.User)
                .Include(a => a.Bids)
                .ThenInclude(b => b.User)
                .Include(a => a.Payment)
                .ToList();
        }
        
        public void Update(Auction auction)
        {
            _context.Auctions.Update(auction);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}