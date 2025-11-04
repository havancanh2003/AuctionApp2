using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class User : IdentityUser<int>
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(255)]
        public string? Address { get; set; }
        [Phone]
        public string? Phone { get; set; }
        public int Role { get; set; } = 2; // RoleTypeEnum
        public bool IsActived { get; set; } = true;
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<Payment> Payment { get; set; } = new List<Payment>();
        public virtual ICollection<Auction> Auctions { get; set; } = new List<Auction>();
    }
}
