using MyApp.Models;
using System.Collections.Generic;

namespace MyApp.Repositories
{
    public interface IAuctionRepository
    {
        Task<IEnumerable<Auction>> GetAllAsync();
        Task<IEnumerable<Auction>> GetApprovedAuctionsAsync();
        Task<IEnumerable<Auction>> GetActiveAuctionsAsync();
        Task<IEnumerable<Auction>> GetAuctionsByUserIdAsync(int userId);
        Task<List<Auction>> GetAllAuctionsWithDetailsAsync();
        Task UpdateStatusAsync(int auctionId, AuctionStatus status);
        Task<Auction> GetByIdAsync(int id);
        Task<Auction> GetByIdWithDetailsAsync(int id);
        Task AddAsync(Auction auction);
        Task UpdateAsync(Auction auction);
        Task SaveAsync();
    }
}
