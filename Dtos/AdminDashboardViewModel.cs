using System.ComponentModel.DataAnnotations;

namespace MyApp.Dtos
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new();
        public int TotalAuctions { get; set; }
        public int ActiveAuctions { get; set; }
        public int TotalBids { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class UserManagementViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();
        public string Search { get; set; } = string.Empty;
        public string SelectedRole { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalUsers { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Họ tên")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Vai trò")]
        public int Role { get; set; }

        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public bool IsActived { get; set; }

        [Display(Name = "Email đã xác thực")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Đăng nhập cuối")]
        public DateTime? LastLoginDate { get; set; }
    }

    public class UserDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActived { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int TotalAuctions { get; set; }
        public int TotalBids { get; set; }
    }

    public class SelectListItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }
}