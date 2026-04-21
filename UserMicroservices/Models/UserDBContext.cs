using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace UserMicroservices.Models
{
    public class UserDBContext : DbContext
    {
        public UserDBContext() { }
        public UserDBContext(DbContextOptions dbContextOptions) { }
        public DbSet<User> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                              .SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json");
            var config = builder.Build();
            var connectionString = config.GetConnectionString("UserDBConnectionString");
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    EmailId = "Franken@gmail.com", 
                    UserPassword = "Franken@785", 
                    RoleName = "Admin",
                    Gender = 'M',
                    DateOfBirth = new DateTime(1978, 9, 10),
                    Address = "Texas, USA"
                },
                new User
                {
                    EmailId = "SamRocks@gmail.com",
                    UserPassword = "Sam@564",
                    RoleName = "User",
                    Gender = 'M',
                    DateOfBirth = new DateTime(1986, 3, 3),
                    Address = "Denver, USA"
                },
                new User
                {
                    EmailId = "PaulGrey@gmail.com",
                    UserPassword = "Paul@123",
                    RoleName = "User",
                    Gender = 'M',
                    DateOfBirth = new DateTime(1993, 7, 7),
                    Address = "Denver, USA"
                }
            );
        }
    }
}
