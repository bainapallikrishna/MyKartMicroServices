using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace PurchaseMicroservices.Models
{
    public class PurchaseDBContext: DbContext
    {
        public PurchaseDBContext(DbContextOptions<PurchaseDBContext> options) : base(options)
        {
        }
        public DbSet<Purchase> Purchases { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                var config = builder.Build();
                var connectionString =
                    config.GetConnectionString("PurchaseDBConnectionString");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

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
