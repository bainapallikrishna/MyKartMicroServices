using CategoryMicroservices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ProductMicroservices.Models
{
    public class CategoryDBContextFactory : IDesignTimeDbContextFactory<CategoryDBContext>
    {
        public CategoryDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CategoryDBContext>();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString =
                configuration.GetConnectionString("CategoryDBConnectionString");

            optionsBuilder.UseSqlServer(connectionString);

            return new CategoryDBContext(optionsBuilder.Options);
        }
    }
}

