using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PurchaseMicroservices.Models
{
    public class PurchaseDBContextFactory : IDesignTimeDbContextFactory<PurchaseDBContext>
    {
        public PurchaseDBContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<PurchaseDBContext>();
            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("PurchaseDBConnectionString"));

            return new PurchaseDBContext(optionsBuilder.Options);
        }
    }
}