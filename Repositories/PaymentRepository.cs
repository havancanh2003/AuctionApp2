using MyApp.Models;
using MyApp.Data;
using Microsoft.EntityFrameworkCore;
namespace MyApp.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Payment> GetAll()
        {
            return _context.Payments.ToList();
        }
         public List<Payment> GetAllPaymentsWithDetails()
    {
        return _context.Payments
            .Include(p => p.User)
            .Include(p => p.Auction)
            .ToList();
    }



        public Payment GetById(int id)
        {
            return _context.Payments.Find(id);
        }

        public void Add(Payment payment)
        {
            _context.Payments.Add(payment);
        }
        public void UpdateStatus(int paymentId, PaymentStatus newStatus)
        {
            var payment = _context.Payments.Find(paymentId);
            if (payment != null)
            {
                payment.Status = newStatus;
                _context.SaveChanges();
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}