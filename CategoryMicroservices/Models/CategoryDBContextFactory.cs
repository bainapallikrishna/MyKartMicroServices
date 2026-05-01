using CategoryMicroservices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CategoryMicroservices.Models
{
    public class CategoryDBContextFactory : IDesignTimeDbContextFactory<CategoryDBContext>
    {
        public CategoryDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CategoryDBContext>();

            // ✅ Reads appsettings.Development.json too!
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString =
                configuration.GetConnectionString("CategoryDBConnectionString");

            optionsBuilder.UseSqlServer(connectionString);
            return new CategoryDBContext(optionsBuilder.Options);
        }
    }
}