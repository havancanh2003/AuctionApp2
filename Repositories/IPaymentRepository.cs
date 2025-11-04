using MyApp.Models;
using System.Collections.Generic;

namespace MyApp.Repositories
{
    public interface IPaymentRepository
    {
        IEnumerable<Payment> GetAll();
        List<Payment> GetAllPaymentsWithDetails();

        Payment GetById(int id);
        void UpdateStatus(int paymentId, PaymentStatus newStatus);
        void Add(Payment payment);
        void Save();
    }
}
