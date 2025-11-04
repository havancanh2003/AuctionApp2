using MyApp.Models;
using System.Collections.Generic;

namespace MyApp.Repositories
{
    public interface IAuctionRepository
    {
        IEnumerable<Auction> GetAll();
        IEnumerable<Auction> GetApprovedAuctions();
        IEnumerable<Auction> GetActiveAuctions();
        List<Auction> GetAllAuctionsWithDetails();
        void UpdateStatus(int auctionId, AuctionStatus status);
        Auction GetById(int id);
        void Add(Auction auction);
        void Update(Auction auction);
        void Save();
    }
}
