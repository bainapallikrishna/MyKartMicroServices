using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ProductMicroservices.Models
{
    public class ProductDBContext : DbContext
    {

        public ProductDBContext(DbContextOptions dbContextOptions)
      : base(dbContextOptions)
        {
            try
            {
                var databaseCreater = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                if (databaseCreater != null)
                {
                    if (!databaseCreater.CanConnect())
                    {
                        databaseCreater.Create();
                    }
                    if (!databaseCreater.HasTables())
                    {
                        databaseCreater.CreateTables();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                          .SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json");
            var config = builder.Build();
            var connectionString = config.GetConnectionString("ProductDBConnectionString");
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // configure Product -> Category relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // seed categories consistent with other microservice
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Motors" },
                new Category { CategoryId = 2, CategoryName = "Arts" },
                new Category { CategoryId = 3, CategoryName = "Furniture" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = "P101",
                    ProductName = "Lamborghini Gallardo Spyder",
                    CategoryId = 1,
                    Price = 18000000,
                    QuantityAvailable = 10
                },
                new Product
                {
                    ProductId = "P102",
                    ProductName = "Harley Davidson Iron 883",
                    CategoryId = 1,
                    Price = 700000,
                    QuantityAvailable = 10
                },
                new Product
                {
                    ProductId = "P103",
                    ProductName = "Abstract Hand painted Oil Painting on Canvas",
                    CategoryId = 2,
                    Price = 2056,
                    QuantityAvailable = 200
                },
                new Product
                {
                    ProductId = "P104",
                    ProductName = "Marble Elephants statue",
                    CategoryId = 2,
                    Price = 9000,
                    QuantityAvailable = 100
                },
                new Product
                {
                    ProductId = "P105",
                    ProductName = "Dining Table",
                    CategoryId = 3,
                    Price = 15000,
                    QuantityAvailable = 50
                }
            );
        }


    }
}
