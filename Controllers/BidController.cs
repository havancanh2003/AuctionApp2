using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MyApp.Models;
using MyApp.Repositories;
using System.Security.Claims;

namespace MyApp.Controllers
{
    [Authorize]
    public class BidController : Controller
    {
        private readonly IBidRepository _bidRepository;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly ILogger<BidController> _logger;

        public BidController(
            IBidRepository bidRepository,
            IAuctionRepository auctionRepository,
            IHubContext<AuctionHub> hubContext,
            ILogger<BidController> logger)
        {
            _bidRepository = bidRepository;
            _auctionRepository = auctionRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(int auctionId, decimal bidAmount)
        {
            try
            {
                // 1. Kiểm tra người dùng đăng nhập
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Json(new { success = false, message = "Người dùng chưa đăng nhập." });
                }

                if (!int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Json(new { success = false, message = "Thông tin người dùng không hợp lệ." });
                }

                // 2. Kiểm tra phiên đấu giá
                var auction = await _auctionRepository.GetByIdWithDetailsAsync(auctionId);
                if (auction == null)
                {
                    return Json(new { success = false, message = "Phiên đấu giá không tồn tại." });
                }

                if (auction.Status != AuctionStatus.Approved)
                {
                    return Json(new { success = false, message = "Phiên đấu giá chưa được phê duyệt." });
                }

                // Kiểm tra thời gian đấu giá
                var now = DateTime.Now;
                if (now < auction.TimeStart)
                {
                    return Json(new { success = false, message = "Phiên đấu giá chưa bắt đầu." });
                }

                if (now > auction.TimeEnd)
                {
                    return Json(new { success = false, message = "Phiên đấu giá đã kết thúc." });
                }

                // 3. Không cho đấu giá chính sản phẩm của mình
                if (auction.UserId == userId)
                {
                    return Json(new { success = false, message = "Bạn không thể đấu giá sản phẩm của chính mình." });
                }

                // 4. Kiểm tra giá đấu
                var highestBid = await _bidRepository.GetHighestBidForAuctionAsync(auctionId);
                var minBidAmount = highestBid != null
                    ? highestBid.Price + 1000 // Tăng ít nhất 1,000 VND
                    : auction.PriceStart;

                if (bidAmount < minBidAmount)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Giá đặt phải cao hơn ít nhất 1,000 VND so với giá hiện tại. Giá tối thiểu: {minBidAmount:N0} VND"
                    });
                }

                // 5. Lưu dữ liệu đấu giá
                var bid = new Bid
                {
                    AuctionID = auctionId,
                    UserID = userId,
                    Price = bidAmount,
                    BidTime = DateTime.Now,
                    IsWinning = true // Mặc định là winning, sẽ update các bid khác
                };

                // Cập nhật tất cả các bid khác trong phiên này thành không winning
                await _bidRepository.UpdateAllBidsToLosingAsync(auctionId);

                await _bidRepository.AddAsync(bid);
                await _bidRepository.SaveAsync();

                // 6. Phát thông tin tới client qua SignalR
                await _hubContext.Clients.Group(auctionId.ToString())
                    .SendAsync("ReceiveNewBid", new
                    {
                        bidId = bid.Id,
                        auctionId = bid.AuctionID,
                        userId = bid.UserID,
                        userName = User.Identity.Name,
                        price = bid.Price,
                        time = bid.BidTime.ToString("HH:mm:ss"),
                        isWinning = true
                    });

                // Cập nhật giá cao nhất
                await _hubContext.Clients.Group(auctionId.ToString())
                    .SendAsync("UpdateHighestBid", new
                    {
                        auctionId = bid.AuctionID,
                        highestPrice = bid.Price,
                        userId = bid.UserID,
                        userName = User.Identity.Name,
                        time = bid.BidTime.ToString("HH:mm:ss")
                    });

                _logger.LogInformation("User {UserId} placed bid {BidAmount} on auction {AuctionId}", userId, bidAmount, auctionId);

                return Json(new
                {
                    success = true,
                    message = "Đặt giá thành công!",
                    highestPrice = bid.Price,
                    bidId = bid.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing bid for auction {AuctionId}", auctionId);
                return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình đặt giá." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var bids = await _bidRepository.GetAllBidsWithDetailsAsync();
            return View(bids);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyBids()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var myBids = await _bidRepository.GetBidsByUserIdAsync(userId);
            return View(myBids);
        }

        [HttpGet]
        public async Task<IActionResult> GetBidHistory(int auctionId)
        {
            var bidHistory = await _bidRepository.GetBidHistoryByAuctionAsync(auctionId);
            return Json(bidHistory.Take(10)); // Trả về 10 bid gần nhất
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentHighestBid(int auctionId)
        {
            var highestBid = await _bidRepository.GetHighestBidForAuctionAsync(auctionId);
            var auction = await _auctionRepository.GetByIdAsync(auctionId);

            var currentPrice = highestBid?.Price ?? auction?.PriceStart ?? 0;
            return Json(new { currentPrice });
        }
    }
}