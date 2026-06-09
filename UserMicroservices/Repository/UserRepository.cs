using System.Linq;
using Microsoft.Extensions.Logging;
using UserMicroservices.Models;

namespace UserMicroservices.Repository
{
    public class UserRepository 
    {
        private readonly UserDBContext dbContext;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(UserDBContext dbContext, ILogger<UserRepository> logger)
        {
            this.dbContext = dbContext;
            _logger = logger;
        }

        public List<User> GetAllUsers()
        {
            List<User> listOfUsers = dbContext.Users.ToList();
            return listOfUsers;
        }

        public bool AddNewUser(User user)
        {
            bool status = false;
            try
            {
                // Hash password before storing
                user.UserPassword = HashPassword(user.UserPassword);
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                status = true;
            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }

        public User? GetUserByEmail(string email)
        {
            _logger.LogInformation("Fetching user by email: {Email}", email);
            // Use SingleOrDefault on EmailId since Find expects the primary key
            var user = dbContext.Users.SingleOrDefault(u => u.EmailId == email);
            if (user == null)
                _logger.LogInformation("User not found: {Email}", email);
            else
                _logger.LogInformation("User found: {Email}", email);
            return user;
        }

        public bool UpdatePassword(string EmailId, string newPassword)
        {
            var user = dbContext.Users.Find(EmailId);
            if (user == null) return false;
            user.UserPassword = HashPassword(newPassword);
            dbContext.Users.Update(user);
            dbContext.SaveChanges();
            return true;
        }
        public int UpdateUserDetails(User user)
        {
            int status = -1;
            User userObj = dbContext.Users.Find(user.EmailId);
            try
            {
                if (userObj != null)
                {
                    userObj.UserPassword = user.UserPassword;
                    userObj.RoleName= user.RoleName;
                    userObj.Gender= user.Gender;
                    userObj.DateOfBirth= user.DateOfBirth;
                    userObj.Address = user.Address;
                    dbContext.Users.Update(userObj);
                    dbContext.SaveChanges();
                    status = 1;
                }
            }
            catch (Exception)
            {
                status = -99;
            }
            return status;
        }
        public bool DeleteUser(string emailId)
        {
            bool status = false;
            User user = dbContext.Users.Find(emailId);

            try
            {
                if (user != null)
                {
                    dbContext.Users.Remove(user);
                    dbContext.SaveChanges();
                    status = true;
                }
            }
            catch (Exception)
            {
                status = false;
            }
            return status;
        }

        private static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
