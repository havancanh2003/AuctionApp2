using Microsoft.AspNetCore.SignalR;
using MyApp.Hubs;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.Repositories;
using Microsoft.AspNetCore.Authorization;


namespace MyApp.Controllers
{
    [Authorize]
    public class BidController : Controller
    {
        private readonly IBidRepository _bidRepository;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IHubContext<AuctionHub> _hubContext;

        public BidController(IBidRepository bidRepository, IAuctionRepository auctionRepository, IHubContext<AuctionHub> hubContext)
        {
            _bidRepository = bidRepository;
            _auctionRepository = auctionRepository;
            _hubContext = hubContext;

        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceBid(int auctionId, decimal bidAmount)
        {
            // 1. Kiểm tra người dùng đăng nhập
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Người dùng chưa đăng nhập.");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return BadRequest("Thông tin người dùng không hợp lệ.");
            }

            // 2. Kiểm tra phiên đấu giá tồn tại
            var auction =  _auctionRepository.GetById(auctionId);
            if (auction == null || auction.Status != AuctionStatus.Approved)
            {
                return BadRequest("Phiên đấu giá không hợp lệ hoặc chưa được phê duyệt.");
            }

            // 3. Không cho đấu giá chính sản phẩm của mình
            if (auction.UserId == userId)
            {
                return BadRequest("Bạn không thể đấu giá sản phẩm của chính mình.");
            }

            // 4. Kiểm tra giá đấu
            var highestBid =  _bidRepository.GetHighestBidForAuction(auctionId);
            var minBidAmount = highestBid != null
                ? highestBid.Price + 1
                : auction.PriceStart;

            if (bidAmount < minBidAmount)
            {
                return BadRequest($"Giá đặt phải cao hơn ít nhất 1 đơn vị so với giá hiện tại: {minBidAmount}");
            }

            // 5. Lưu dữ liệu đấu giá
            var bid = new Bid
            {
                AuctionID = auctionId,
                UserID = userId,
                Price = bidAmount,
                Time = DateTime.Now
            };

             _bidRepository.Add(bid);
             _bidRepository.Save();

            // 6. Phát thông tin tới client qua SignalR
            await _hubContext.Clients.Group(auctionId.ToString())
                .SendAsync("ReceiveHighestBid", new
                {
                    auctionId,
                    highestPrice = bid.Price,
                    userId = bid.UserID,
                    time = bid.Time.ToString("HH:mm:ss")
                });

            return Ok(new
            {
                message = "Đặt giá thành công",
                highestPrice = bid.Price
            });
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userRoleClaim = User.FindFirst(ClaimTypes.Role);
            if (userRoleClaim == null)
            {
                return Unauthorized("Không xác định được vai trò người dùng.");
            }

            if (!int.TryParse(userRoleClaim.Value, out int role))
            {
                return BadRequest("Giá trị vai trò không hợp lệ.");
            }

            // Giả sử RoleTypeEnum.Admin = 1
            if (role != (int)RoleTypeEnum.Admin)
            {
                return Forbid("Bạn không có quyền truy cập trang này.");
            }

            var bids = _bidRepository.GetAllBilsWithDetails();
            return View(bids);
        }
    }
}