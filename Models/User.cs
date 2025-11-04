using Microsoft.AspNetCore.Identity;
using MyApp.Dtos;
using MyApp.Models;
using System.ComponentModel.DataAnnotations;

public class User : IdentityUser<int>
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Address { get; set; }

    [Phone]
    public string? Phone { get; set; }

    public int Role { get; set; } = (int)RoleTypeEnum.Customer;

    public bool IsActived { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? LastLoginDate { get; set; }

    // Navigation properties
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<Payment> Payment { get; set; } = new List<Payment>();
    public virtual ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}