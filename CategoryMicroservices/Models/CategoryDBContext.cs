using Microsoft.EntityFrameworkCore;

namespace CategoryMicroservices.Models
{
    public class CategoryDBContext : DbContext
    {
        public CategoryDBContext(DbContextOptions<CategoryDBContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }

        // ✅ REMOVED OnConfiguring — connection comes from Program.cs

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