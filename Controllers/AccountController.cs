using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Dtos;
using MyApp.Models;
using System.Security.Claims;

namespace MyApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng");
                    return View(model);
                }

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name.Trim(),
                    Role = (int)model.RoleType,
                    EmailConfirmed = true,
                    IsActived = true,
                    PhoneNumber = model.Phone,
                    Address = model.Address
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Ensure role exists
                    var roleName = model.RoleType.ToString();
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                    }

                    // Add user to role
                    await _userManager.AddToRoleAsync(user, roleName);

                    // Add additional claims
                    await _userManager.AddClaimAsync(user, new Claim("Name", user.Name));
                    await _userManager.AddClaimAsync(user, new Claim("Role", user.Role.ToString()));

                    // Sign in user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    _logger.LogInformation("User {Email} registered successfully as {Role}", user.Email, roleName);

                    // Redirect based on role
                    return RedirectToLocal(returnUrl);
                }

                // Handle identity errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, GetUserFriendlyErrorMessage(error));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại.");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Clear existing external cookie to ensure a clean login process
            HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
                    return View(model);
                }

                if (!user.IsActived)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", user.Email);

                    // Update last login time
                    user.LastLoginDate = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    return RedirectToLocal(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction("LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account {Email} locked out", user.Email);
                    return RedirectToAction("Lockout");
                }

                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity.Name;
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {UserName} logged out", userName);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Address = user.Address
            };

            return View(model);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Redirect based on user role
            //if (User.IsInRole("Admin"))
            //{
            //    return RedirectToAction("Index", "Admin");
            //}

            return RedirectToAction("Index", "Home");
        }

        private string GetUserFriendlyErrorMessage(IdentityError error)
        {
            return error.Code switch
            {
                "PasswordTooShort" => "Mật khẩu phải có ít nhất 6 ký tự",
                "PasswordRequiresNonAlphanumeric" => "Mật khẩu phải có ít nhất 1 ký tự đặc biệt",
                "PasswordRequiresDigit" => "Mật khẩu phải có ít nhất 1 chữ số",
                "PasswordRequiresLower" => "Mật khẩu phải có ít nhất 1 chữ thường",
                "PasswordRequiresUpper" => "Mật khẩu phải có ít nhất 1 chữ hoa",
                "DuplicateUserName" => "Email đã được sử dụng",
                _ => error.Description
            };
        }
    }
}