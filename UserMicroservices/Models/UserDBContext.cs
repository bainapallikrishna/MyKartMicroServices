using Microsoft.EntityFrameworkCore;

namespace UserMicroservices.Models
{
    public class UserDBContext : DbContext
    {
        public UserDBContext(DbContextOptions<UserDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.EmailId);
                entity.Property(e => e.EmailId).HasColumnName("EmailId");
                entity.Property(e => e.UserPassword).HasColumnName("UserPassword");
                entity.Property(e => e.RoleName).HasColumnName("RoleName");
                entity.Property(e => e.FailedLoginAttempts).HasColumnName("FailedLoginAttempts");
                entity.Property(e => e.LockoutEnd).HasColumnName("LockoutEnd");
            });

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

            base.OnModelCreating(modelBuilder);
        }
    }
}