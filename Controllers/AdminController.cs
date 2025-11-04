using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Dtos;

namespace MyApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                //var stats = await GetDashboardStatistics();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải dashboard.";
                return View(new AdminDashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> UserManagement(string search = "", string role = "", int page = 1, int pageSize = 10)
        {
            try
            {
                var usersQuery = _userManager.Users.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    usersQuery = usersQuery.Where(u =>
                        u.Name.Contains(search) ||
                        u.Email.Contains(search) ||
                        u.PhoneNumber.Contains(search));
                }

                // Apply role filter
                if (!string.IsNullOrEmpty(role) && role != "All")
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                    var userIds = usersInRole.Select(u => u.Id);
                    usersQuery = usersQuery.Where(u => userIds.Contains(u.Id));
                }

                var totalUsers = await usersQuery.CountAsync();
                var users = await usersQuery
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserViewModel
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Phone = u.PhoneNumber,
                        Address = u.Address,
                        Role = u.Role,
                        IsActived = u.IsActived,
                        CreatedAt = u.CreatedAt,
                        LastLoginDate = u.LastLoginDate,
                        EmailConfirmed = u.EmailConfirmed
                    })
                    .ToListAsync();

                // Get roles for each user
                foreach (var user in users)
                {
                    var userEntity = await _userManager.FindByIdAsync(user.Id.ToString());
                    var roles = await _userManager.GetRolesAsync(userEntity);
                    user.RoleName = roles.FirstOrDefault() ?? "Customer";
                }

                var model = new UserManagementViewModel
                {
                    Users = users,
                    Search = search,
                    SelectedRole = role,
                    Page = page,
                    PageSize = pageSize,
                    TotalUsers = totalUsers,
                    TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize)
                };

                ViewBag.Roles = await GetRoleList();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user management page");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách người dùng.";
                return View(new UserManagementViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userAuctions = await GetUserAuctionsCount(id);
                var userBids = await GetUserBidsCount(id);

                var model = new UserDetailViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Address = user.Address,
                    Role = user.Role,
                    RoleName = roles.FirstOrDefault() ?? "Customer",
                    IsActived = user.IsActived,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginDate = user.LastLoginDate,
                    TotalAuctions = userAuctions,
                    TotalBids = userBids
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for user {UserId}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin người dùng.";
                return RedirectToAction(nameof(UserManagement));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại." });
                }

                user.IsActived = !user.IsActived;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var action = user.IsActived ? "kích hoạt" : "khóa";
                    _logger.LogInformation("Admin {AdminUser} {Action} user {UserId}", User.Identity.Name, action, id);
                    return Json(new { success = true, message = $"Đã {action} tài khoản thành công!", isActive = user.IsActived });
                }

                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status for user {UserId}", id);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(int id, int newRole)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại." });
                }

                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove from current roles
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật vai trò." });
                }

                // Add to new role
                var roleName = ((RoleTypeEnum)newRole).ToString();
                var addResult = await _userManager.AddToRoleAsync(user, roleName);

                if (addResult.Succeeded)
                {
                    // Update user's role field
                    user.Role = newRole;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("Admin {AdminUser} updated role for user {UserId} to {Role}",
                        User.Identity.Name, id, roleName);

                    return Json(new
                    {
                        success = true,
                        message = "Cập nhật vai trò thành công!",
                        roleName = roleName
                    });
                }

                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật vai trò." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role for user {UserId}", id);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật vai trò." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại." });
                }

                // Prevent admin from deleting themselves
                if (user.Id == int.Parse(_userManager.GetUserId(User)))
                {
                    return Json(new { success = false, message = "Bạn không thể xóa tài khoản của chính mình." });
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin {AdminUser} deleted user {UserId}", User.Identity.Name, id);
                    return Json(new { success = true, message = "Xóa người dùng thành công!" });
                }

                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa người dùng." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa người dùng." });
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> GetUserStatistics()
        //{
        //    try
        //    {
        //        var stats = await GetDashboardStatistics();
        //        return Json(stats);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting user statistics");
        //        return Json(new { error = "Đã xảy ra lỗi khi lấy thống kê." });
        //    }
        //}

        private async Task<List<SelectListItem>> GetRoleList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "All", Text = "Tất cả vai trò" },
                new SelectListItem { Value = "Admin", Text = "Quản trị viên" },
                new SelectListItem { Value = "Seller", Text = "Người bán" },
                new SelectListItem { Value = "Customer", Text = "Người mua" }
            };
        }

        private async Task<int> GetUserAuctionsCount(int userId)
        {
            // You'll need to inject your ApplicationDbContext for this
            // For now, return 0 - implement based on your data context
            return 0;
        }

        private async Task<int> GetUserBidsCount(int userId)
        {
            // You'll need to inject your ApplicationDbContext for this
            // For now, return 0 - implement based on your data context
            return 0;
        }
    }
}