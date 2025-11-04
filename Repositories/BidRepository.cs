using MyApp.Models;
using MyApp.Data;
using Microsoft.EntityFrameworkCore;
namespace MyApp.Repositories
{
    public class BidRepository : IBidRepository
    {
        private readonly ApplicationDbContext _context;

        public BidRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Bid> GetAll()
        {
            return _context.Bids.ToList();
        }

        public Bid GetById(int id)
        {
            return _context.Bids.Find(id);
        }
        public Bid? GetHighestBidForAuction(int id)
        {
            return _context.Bids
                .Where(b => b.AuctionID == id)
                .OrderByDescending(b => b.Price)
                .FirstOrDefault();
        }
        public List<Bid> GetAllBilsWithDetails()
        {
            return _context.Bids
                .Include(b => b.Auction)
                .Include(b => b.User)
                .ToList();
        }
        public void Add(Bid bid)
        {
            _context.Bids.Add(bid);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}