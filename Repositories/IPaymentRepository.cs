using Microsoft.EntityFrameworkCore;
using MyApp.Models;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<List<Payment>> GetAllPaymentsWithDetailsAsync();
    Task<List<Payment>> GetPaymentsByUserIdAsync(int userId);
    Task<Payment> GetByIdAsync(int id);
    Task<Payment> GetPaymentByAuctionIdAsync(int auctionId);
    Task<Dictionary<string, int>> GetPaymentStatisticsAsync();
    Task UpdateStatusAsync(int paymentId, PaymentStatus newStatus);
    Task AddAsync(Payment payment);
    Task SaveAsync();
}