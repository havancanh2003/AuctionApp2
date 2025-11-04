using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments.ToListAsync();
    }

    public async Task<List<Payment>> GetAllPaymentsWithDetailsAsync()
    {
        return await _context.Payments
            .Include(p => p.User)
            .Include(p => p.Auction)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetPaymentsByUserIdAsync(int userId)
    {
        return await _context.Payments
            .Include(p => p.Auction)
            .Where(p => p.UserID == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Payment> GetByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.User)
            .Include(p => p.Auction)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment> GetPaymentByAuctionIdAsync(int auctionId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.AuctionID == auctionId);
    }

    public async Task<Dictionary<string, int>> GetPaymentStatisticsAsync()
    {
        var total = await _context.Payments.CountAsync();
        var unpaid = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Unpaid);
        var inTransit = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.InTransit);
        var complete = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Complete);

        return new Dictionary<string, int>
        {
            ["total"] = total,
            ["unpaid"] = unpaid,
            ["inTransit"] = inTransit,
            ["complete"] = complete
        };
    }

    public async Task UpdateStatusAsync(int paymentId, PaymentStatus newStatus)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment != null)
        {
            payment.Status = newStatus;
            payment.UpdatedAt = DateTime.Now;
            _context.Payments.Update(payment);
        }
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}