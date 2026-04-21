using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserMicroservices.Models;
using UserMicroservices.Repository;

namespace UserMicroservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserMicroservicesController : Controller
    {
        UserRepository repository;
        public UserMicroservicesController(UserRepository userRepository)
        {
            repository = userRepository;
        }

        [HttpGet]
        public JsonResult GetAllUsersDetails()
        {
            List<User> listOfUsers = repository.GetAllUsers();
            return Json(listOfUsers);
        }

        [HttpGet("{id}")]
        public JsonResult GetUserById(string id)
        {
            List<User> listOfUsers = repository.GetAllUsers();
            User user = listOfUsers.Find(u => u.EmailId == id);
            return Json(user);
        }

        [HttpPost]
        public JsonResult AddNewUser(User user)
        {
            return Json(repository.AddNewUser(user));
        }

        [HttpPut]
        public JsonResult UpdateUser(User user)
        {
            int result = repository.UpdateUserDetails(user);
            return Json(result);
        }

        [HttpDelete("{id}")]
        public JsonResult DeleteUser(string id)
        {
            bool result = repository.DeleteUser(id);
            return Json(result);
        }
    }
}
