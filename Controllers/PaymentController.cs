
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.Repositories;

namespace MyApp.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
   private readonly IPaymentRepository _paymentRepository;

        public PaymentController(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public IActionResult Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            Console.WriteLine("User ID Claim: " + (userIdClaim != null ? userIdClaim.Value : "None"));
            var userRoleClaim = User.FindFirst(ClaimTypes.Role);
            Console.WriteLine("User Role Claim: " + (userRoleClaim != null ? userRoleClaim.Value : "None"));
            var role = int.Parse(userRoleClaim.Value);
            if (role != 1)
            {
                return Forbid();
            }
            var payments = _paymentRepository.GetAllPaymentsWithDetails();
            return View(payments);
        }
        public IActionResult UpdatePaymentStatus(int paymentId, PaymentStatus newStatus)
        {
            _paymentRepository.UpdateStatus(paymentId, newStatus);
            return Ok();
        }
    }
}