using MyApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Bid
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Auction")]
    public int AuctionID { get; set; }

    [ForeignKey("User")]
    public int UserID { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public DateTime BidTime { get; set; }

    public bool IsWinning { get; set; } = true;
    // THÊM M?I: BidCode v?i validation 8 ký t? + ít nh?t 1 s?
    [Required]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "BidCode ph?i có ?úng 8 ký t?")]
    [RegularExpression(@"^(?=.*\d)[A-Za-z\d]{8}$",
        ErrorMessage = "BidCode ph?i có 8 ký t? và ít nh?t 1 s?")]
    [Display(Name = "Mã ??u Giá")]
    public string BidCode { get; set; } = string.Empty;
    // Navigation properties
    public virtual Auction Auction { get; set; }
    public virtual User User { get; set; }
}