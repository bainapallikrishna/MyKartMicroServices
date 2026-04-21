using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace CategoryMicroservices.Models
{
    public class CategoryDBContext : DbContext
    {
  

        public CategoryDBContext(DbContextOptions<CategoryDBContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                var config = builder.Build();
                var connectionString =
                    config.GetConnectionString("CategoryDBConnectionString");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Motors" },
                new Category { CategoryId = 2, CategoryName = "Arts" },
                new Category { CategoryId = 3, CategoryName = "Furniture" }
            );
        }
    }
}
