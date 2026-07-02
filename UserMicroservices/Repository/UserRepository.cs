using System.Linq;
using Microsoft.Extensions.Logging;
using UserMicroservices.Models;
using SharedLibrary.Common;
using System.Diagnostics;

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
            _logger.LogDebug("Database query 'GetAllUsers' started");
            var sw = Stopwatch.StartNew();
            try
            {
                List<User> listOfUsers = dbContext.Users.ToList();
                sw.Stop();
                _logger.LogInformation("Database query 'GetAllUsers' completed in {ElapsedMs}ms - Retrieved {UserCount} users", sw.ElapsedMilliseconds, listOfUsers?.Count ?? 0);
                return listOfUsers;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Database query 'GetAllUsers' failed after {ElapsedMs}ms", sw.ElapsedMilliseconds);
                throw;
            }
        }

        public bool AddNewUser(User user)
        {
            bool status = false;
            _logger.LogDebug("Adding new user with email: {UserEmail}", user?.EmailId ?? "unknown");
            var sw = Stopwatch.StartNew();
            try
            {
                // Hash password before storing
                user.UserPassword = HashPassword(user.UserPassword);
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                status = true;
                sw.Stop();
                _logger.LogInformation("User added successfully with email: {UserEmail} in {ElapsedMs}ms", user?.EmailId, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                status = false;
                _logger.LogError(ex, "Error adding user with email: {UserEmail} after {ElapsedMs}ms", user?.EmailId ?? "unknown", sw.ElapsedMilliseconds);
            }
            return status;
        }

        public User? GetUserByEmail(string email)
        {
            _logger.LogDebug("Database query 'GetUserByEmail' started with email: {Email}", email);
            var sw = Stopwatch.StartNew();
            try
            {
                // Use SingleOrDefault on EmailId since Find expects the primary key
                var user = dbContext.Users.SingleOrDefault(u => u.EmailId == email);
                sw.Stop();
                if (user == null)
                {
                    _logger.LogInformation("User not found for email: {Email} - Query time: {ElapsedMs}ms", email, sw.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogInformation("User found for email: {Email} - Query time: {ElapsedMs}ms", email, sw.ElapsedMilliseconds);
                }
                return user;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error fetching user by email: {Email} after {ElapsedMs}ms", email, sw.ElapsedMilliseconds);
                throw;
            }
        }

        public bool UpdatePassword(string EmailId, string newPassword)
        {
            _logger.LogDebug("Updating password for user: {Email}", EmailId);
            var sw = Stopwatch.StartNew();
            try
            {
                var user = dbContext.Users.Find(EmailId);
                if (user == null)
                {
                    sw.Stop();
                    _logger.LogWarning("Cannot update password - User not found: {Email}", EmailId);
                    return false;
                }
                user.UserPassword = HashPassword(newPassword);
                dbContext.Users.Update(user);
                dbContext.SaveChanges();
                sw.Stop();
                _logger.LogInformation("Password updated successfully for user: {Email} in {ElapsedMs}ms", EmailId, sw.ElapsedMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error updating password for user: {Email} after {ElapsedMs}ms", EmailId, sw.ElapsedMilliseconds);
                return false;
            }
        }

        public int UpdateUserDetails(User user)
        {
            int status = -1;
            _logger.LogDebug("Updating user details for email: {Email}", user?.EmailId ?? "unknown");
            var sw = Stopwatch.StartNew();
            try
            {
                User userObj = dbContext.Users.Find(user.EmailId);
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
                    sw.Stop();
                    _logger.LogInformation("User details updated successfully for email: {Email} in {ElapsedMs}ms", user.EmailId, sw.ElapsedMilliseconds);
                }
                else
                {
                    sw.Stop();
                    _logger.LogWarning("Cannot update user - User not found: {Email}", user?.EmailId);
                    status = -1;
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                status = -99;
                _logger.LogError(ex, "Error updating user details for email: {Email} after {ElapsedMs}ms", user?.EmailId ?? "unknown", sw.ElapsedMilliseconds);
            }
            return status;
        }

        public bool DeleteUser(string emailId)
        {
            bool status = false;
            _logger.LogDebug("Deleting user: {Email}", emailId);
            var sw = Stopwatch.StartNew();
            try
            {
                User user = dbContext.Users.Find(emailId);
                if (user != null)
                {
                    dbContext.Users.Remove(user);
                    dbContext.SaveChanges();
                    status = true;
                    sw.Stop();
                    _logger.LogInformation("User deleted successfully: {Email} in {ElapsedMs}ms", emailId, sw.ElapsedMilliseconds);
                }
                else
                {
                    sw.Stop();
                    _logger.LogWarning("Cannot delete user - User not found: {Email}", emailId);
                    status = false;
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                status = false;
                _logger.LogError(ex, "Error deleting user: {Email} after {ElapsedMs}ms", emailId, sw.ElapsedMilliseconds);
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
