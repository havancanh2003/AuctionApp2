
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Payment> Payments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payment)
                .HasForeignKey(p => p.UserID);

            modelBuilder.Entity<Payment>()
                    .HasOne(p => p.Auction)
                    .WithOne(a => a.Payment)
                    .HasForeignKey<Payment>(p => p.AuctionID);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.UserID);
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Auction)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.AuctionID);
            modelBuilder.Entity<Auction>()
            .HasOne(a => a.User)
            .WithMany(u => u.Auctions)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
