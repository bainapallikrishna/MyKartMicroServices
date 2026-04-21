using UserMicroservices.Models;

namespace UserMicroservices.Repository
{
    public class UserRepository
    {
        UserDBContext dbContext;
        public UserRepository()
        {
        }
        public UserRepository(UserDBContext context)
        {
            dbContext = context;
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
    }
}
