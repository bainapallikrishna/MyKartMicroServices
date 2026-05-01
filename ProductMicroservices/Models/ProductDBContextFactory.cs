using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ProductMicroservices.Models
{
    public class ProductDBContextFactory : IDesignTimeDbContextFactory<ProductDBContext>
    {
        public ProductDBContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ProductDBContext>();
            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("ProductDBConnectionString"));

            return new ProductDBContext(optionsBuilder.Options);
        }
    }
}