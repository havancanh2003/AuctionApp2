using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Models
{
    public class Bid
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Auction")]
        public int AuctionID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public DateTime Time { get; set; }

        // Navigation properties
        public virtual Auction Auction { get; set; }
        public virtual User User { get; set; }
    }
}