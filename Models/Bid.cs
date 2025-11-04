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

    // Navigation properties
    public virtual Auction Auction { get; set; }
    public virtual User User { get; set; }
}