using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UserMicroservices.Models
{
    public class UserDBContextFactory : IDesignTimeDbContextFactory<UserDBContext>
    {
        public UserDBContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<UserDBContext>();
            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("UserDBConnectionString"));

            return new UserDBContext(optionsBuilder.Options);
        }
    }
}