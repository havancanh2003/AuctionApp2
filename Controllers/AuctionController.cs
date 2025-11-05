using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Dtos;
using MyApp.Models;
using MyApp.Repositories;
using System.Security.Claims;

public class AuctionController : Controller
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IBidRepository _bidRepository;
    private readonly ILogger<AuctionController> _logger;

    public AuctionController(
        IAuctionRepository auctionRepository,
        IBidRepository bidRepository,
        ILogger<AuctionController> logger)
    {
        _auctionRepository = auctionRepository;
        _bidRepository = bidRepository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        _logger.LogInformation("User {User} accessed Create Auction page", User.Identity.Name);
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var auction = await _auctionRepository.GetByIdAsync(id);
        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found", id);
            return NotFound();
        }

        var highestBid = await _bidRepository.GetHighestBidForAuctionAsync(id);
        ViewBag.HighestBid = highestBid?.Price ?? auction.PriceStart;

        _logger.LogInformation("Displaying details for auction: {AuctionName}", auction.Name);
        return View(auction);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(Auction auction)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        // Validation
        if (auction.TimeEnd <= auction.TimeStart)
        {
            ModelState.AddModelError(nameof(auction.TimeEnd), "Thời gian kết thúc phải sau thời gian bắt đầu");
        }

        if (auction.TimeStart <= DateTime.Now)
        {
            ModelState.AddModelError(nameof(auction.TimeStart), "Thời gian bắt đầu phải trong tương lai");
        }

        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Model validation error: {ErrorMessage}", error.ErrorMessage);
            }
            return View(auction);
        }

        auction.UserId = int.Parse(userIdClaim.Value);
        auction.Status = AuctionStatus.Pending;

        await _auctionRepository.AddAsync(auction);
        await _auctionRepository.SaveAsync();

        _logger.LogInformation("User {User} created new auction: {AuctionId}", User.Identity.Name, auction.Id);
        TempData["SuccessMessage"] = "Tạo phiên đấu giá thành công! Chờ quản trị viên phê duyệt.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ApprovedAuctions()
    {
        var approvedAuctions = await _auctionRepository.GetApprovedAuctionsAsync();
        return View(approvedAuctions);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ActiveAuctions()
    {
        var activeAuctions = await _auctionRepository.GetActiveAuctionsAsync();
        return View(activeAuctions);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var auctions = await _auctionRepository.GetAllAuctionsWithDetailsAsync();
        return View(auctions);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int auctionId, AuctionStatus status)
    {
        await _auctionRepository.UpdateStatusAsync(auctionId, status);
        await _auctionRepository.SaveAsync();

        _logger.LogInformation("Updated Auction ID {AuctionId} to Status {Status}", auctionId, status);
        return Ok(new { success = true, message = "Cập nhật trạng thái thành công!" });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyAuctions()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var myAuctions = await _auctionRepository.GetAuctionsByUserIdAsync(userId);
        return View(myAuctions);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> BiddingHistory()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var biddingHistory = await _bidRepository.GetBidHistoryByAuctionAsync(userId);
        return View(biddingHistory);
    }
    //[HttpPost]
    //[Authorize]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> DeleteAuction(int id)
    //{
    //    try
    //    {
    //        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    //        var auction = await _auctionRepository.GetByIdAsync(id);

    //        if (auction == null)
    //        {
    //            return Json(new { success = false, message = "Phiên đấu giá không tồn tại." });
    //        }

    //        // Check if user owns the auction
    //        if (auction.UserId != userId)
    //        {
    //            return Json(new { success = false, message = "Bạn không có quyền xóa phiên đấu giá này." });
    //        }

    //        // Only allow deletion for pending or rejected auctions
    //        if (auction.Status != AuctionStatus.Pending && auction.Status != AuctionStatus.Rejected)
    //        {
    //            return Json(new { success = false, message = "Chỉ có thể xóa phiên đấu giá đang chờ duyệt hoặc bị từ chối." });
    //        }

    //        _auctionRepository.Delete(auction);
    //        await _auctionRepository.SaveAsync();

    //        _logger.LogInformation("User {UserId} deleted auction {AuctionId}", userId, id);
    //        return Json(new { success = true, message = "Xóa phiên đấu giá thành công!" });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error deleting auction {AuctionId}", id);
    //        return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa phiên đấu giá." });
    //    }
    //}

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var auction = await _auctionRepository.GetByIdAsync(id);

            if (auction == null)
            {
                return NotFound();
            }

            if (auction.UserId != userId)
            {
                return Forbid();
            }

            // Only allow editing for pending auctions
            if (auction.Status != AuctionStatus.Pending)
            {
                TempData["ErrorMessage"] = "Chỉ có thể chỉnh sửa phiên đấu giá đang chờ duyệt.";
                return RedirectToAction(nameof(MyAuctions));
            }

            // Map to edit view model
            var model = new AuctionEditViewModel
            {
                Id = auction.Id,
                Name = auction.Name,
                Description = auction.Description,
                ImgUrl = auction.ImgUrl,
                PriceStart = auction.PriceStart,
                TimeStart = auction.TimeStart,
                TimeEnd = auction.TimeEnd
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit page for auction {AuctionId}", id);
            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải trang chỉnh sửa.";
            return RedirectToAction(nameof(MyAuctions));
        }
    }
}