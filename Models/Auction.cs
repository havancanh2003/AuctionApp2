using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Models
{
    public enum AuctionStatus
    {
        Pending =1,
        Approved =2,
        Rejected =3,
        Closed =4
    }

    public class Auction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // Người đăng sản phẩm

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string ImgUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceStart { get; set; }

        public DateTime TimeStart { get; set; }

        public DateTime TimeEnd { get; set; }

        public AuctionStatus Status { get; set; } = AuctionStatus.Pending;
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public Payment? Payment { get; set; }
        public virtual User? User { get; set; }
    }
}