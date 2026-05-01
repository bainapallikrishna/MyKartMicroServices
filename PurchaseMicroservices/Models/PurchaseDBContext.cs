using Microsoft.EntityFrameworkCore;

namespace PurchaseMicroservices.Models
{
    public class PurchaseDBContext : DbContext
    {
        public PurchaseDBContext(DbContextOptions<PurchaseDBContext> options) : base(options) { }

        public DbSet<Purchase> Purchases { get; set; }

        // ✅ REMOVED OnConfiguring — connection comes from Program.cs

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Purchase>()
                .Property(p => p.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Purchase>().HasData(
                new Purchase { PurchaseId = 1, EmailId = "test1@gmail.com", ProductId = "P001", QuantityPurchased = 2, TotalPrice = 19.98m },
                new Purchase { PurchaseId = 2, EmailId = "test2@gmail.com", ProductId = "P002", QuantityPurchased = 1, TotalPrice = 9.99m },
                new Purchase { PurchaseId = 3, EmailId = "test3@gmail.com", ProductId = "P003", QuantityPurchased = 3, TotalPrice = 29.97m }
            );
        }
    }
}