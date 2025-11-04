using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Models;
using MyApp.Repositories;
using System.Security.Claims;

namespace MyApp.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentRepository paymentRepository,
            IAuctionRepository auctionRepository,
            IBidRepository bidRepository,
            ILogger<PaymentController> logger)
        {
            _paymentRepository = paymentRepository;
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var payments = await _paymentRepository.GetAllPaymentsWithDetailsAsync();
                return View(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for admin");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách thanh toán.";
                return View(new List<Payment>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int paymentId, PaymentStatus newStatus)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin thanh toán." });
                }

                // Validate status transition
                if (!IsValidStatusTransition(payment.Status, newStatus))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Chuyển trạng thái không hợp lệ."
                    });
                }

                await _paymentRepository.UpdateStatusAsync(paymentId, newStatus);
                await _paymentRepository.SaveAsync();

                _logger.LogInformation("Payment {PaymentId} status updated to {NewStatus} by {User}",
                    paymentId, newStatus, User.Identity.Name);

                return Json(new
                {
                    success = true,
                    message = "Cập nhật trạng thái thành công!",
                    statusText = GetStatusText(newStatus),
                    statusClass = GetStatusClass(newStatus)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for {PaymentId}", paymentId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái." });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyPayments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized();
                }

                var myPayments = await _paymentRepository.GetPaymentsByUserIdAsync(userId);
                return View(myPayments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for user");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách thanh toán.";
                return View(new List<Payment>());
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayment(int auctionId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Json(new { success = false, message = "Người dùng không hợp lệ." });
                }

                // Check if auction exists and is completed
                var auction = await _auctionRepository.GetByIdWithDetailsAsync(auctionId);
                if (auction == null)
                {
                    return Json(new { success = false, message = "Phiên đấu giá không tồn tại." });
                }

                if (auction.Status != AuctionStatus.Closed)
                {
                    return Json(new { success = false, message = "Phiên đấu giá chưa kết thúc." });
                }

                // Check if user is the winner
                var winningBid = await _bidRepository.GetHighestBidForAuctionAsync(auctionId);
                if (winningBid == null || winningBid.UserID != userId)
                {
                    return Json(new { success = false, message = "Bạn không phải là người thắng cuộc của phiên đấu giá này." });
                }

                // Check if payment already exists
                var existingPayment = await _paymentRepository.GetPaymentByAuctionIdAsync(auctionId);
                if (existingPayment != null)
                {
                    return Json(new { success = false, message = "Đã tồn tại thanh toán cho phiên đấu giá này." });
                }

                // Create payment
                var payment = new Payment
                {
                    UserID = userId,
                    AuctionID = auctionId,
                    Price = winningBid.Price,
                    Status = PaymentStatus.Unpaid,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _paymentRepository.AddAsync(payment);
                await _paymentRepository.SaveAsync();

                _logger.LogInformation("Payment created for auction {AuctionId} by user {UserId}",
                    auctionId, userId);

                return Json(new
                {
                    success = true,
                    message = "Tạo thanh toán thành công!",
                    paymentId = payment.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for auction {AuctionId}", auctionId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo thanh toán." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaymentStatistics()
        {
            try
            {
                var statistics = await _paymentRepository.GetPaymentStatisticsAsync();
                return Json(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment statistics");
                return Json(new { error = "Đã xảy ra lỗi khi lấy thống kê." });
            }
        }

        private bool IsValidStatusTransition(PaymentStatus currentStatus, PaymentStatus newStatus)
        {
            // Define valid status transitions
            var validTransitions = new Dictionary<PaymentStatus, List<PaymentStatus>>
            {
                [PaymentStatus.Unpaid] = new List<PaymentStatus> { PaymentStatus.InTransit, PaymentStatus.Complete },
                [PaymentStatus.InTransit] = new List<PaymentStatus> { PaymentStatus.Complete, PaymentStatus.Unpaid },
                [PaymentStatus.Complete] = new List<PaymentStatus> { } // Cannot change from complete
            };

            return validTransitions.ContainsKey(currentStatus) &&
                   validTransitions[currentStatus].Contains(newStatus);
        }

        private string GetStatusText(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Unpaid => "Chưa thanh toán",
                PaymentStatus.InTransit => "Đang xử lý",
                PaymentStatus.Complete => "Hoàn thành",
                _ => status.ToString()
            };
        }

        private string GetStatusClass(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Unpaid => "warning",
                PaymentStatus.InTransit => "info",
                PaymentStatus.Complete => "success",
                _ => "secondary"
            };
        }
    }
}