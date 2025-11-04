using MyApp.Models;
using System.Collections.Generic;

namespace MyApp.Repositories
{
    public interface IBidRepository
    {
        IEnumerable<Bid> GetAll();
        Bid GetById(int id);
        Bid GetHighestBidForAuction(int id);
        List<Bid> GetAllBilsWithDetails();
        void Add(Bid bid);
        void Save();
    }
}
