using System.ComponentModel.DataAnnotations;

namespace MyApp.Dtos
{
    public class AuctionEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên phiên đấu giá không được để trống")]
        [StringLength(200, ErrorMessage = "Tên phiên đấu giá không được vượt quá 200 ký tự")]
        [Display(Name = "Tên phiên đấu giá")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL hình ảnh không được để trống")]
        [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
        [Display(Name = "URL hình ảnh")]
        public string ImgUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá khởi điểm không được để trống")]
        [Range(1000, double.MaxValue, ErrorMessage = "Giá khởi điểm phải lớn hơn 1,000 VND")]
        [Display(Name = "Giá khởi điểm")]
        public decimal PriceStart { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTime TimeStart { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        [Display(Name = "Thời gian kết thúc")]
        public DateTime TimeEnd { get; set; }
    }
}
