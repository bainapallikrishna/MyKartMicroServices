using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserMicroservices.Models;
using UserMicroservices.Repository;
using SharedLibrary.Common;
using Microsoft.Extensions.Logging;

namespace UserMicroservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class UserMicroservicesController : Controller
    {
        private readonly UserRepository _repository;
        private readonly ILogger<UserMicroservicesController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserMicroservicesController(
            UserRepository userRepository,
            ILogger<UserMicroservicesController> logger,
            IHttpContextAccessor contextAccessor)
        {
            _repository = userRepository;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        private string GetCorrelationId() => _contextAccessor.GetCorrelationId();

        [HttpGet]
        [Cacheable(durationInSeconds: 300)]
        public JsonResult GetAllUsersDetails()
        {
            var correlationId = GetCorrelationId();
            using (var perfLogger = new PerformanceLogger(_logger, "GetAllUsersDetails", correlationId))
            {
                try
                {
                    _logger.LogInformation("Fetching all users - CorrelationId: {CorrelationId}", correlationId);
                    // Use CQRS query
                    var query = new UserMicroservices.CQRS.Queries.GetAllUsersQuery();
                    var dispatcher = HttpContext.RequestServices.GetService(typeof(SharedLibrary.CQRS.IDispatcher)) as SharedLibrary.CQRS.IDispatcher;
                    var listOfUsers = dispatcher.Send<UserMicroservices.CQRS.Queries.GetAllUsersQuery, System.Collections.Generic.List<User>>(query).GetAwaiter().GetResult();
                    perfLogger.LogSuccess();
                    _logger.LogInformation("Successfully fetched {UserCount} users - CorrelationId: {CorrelationId}", listOfUsers?.Count ?? 0, correlationId);
                    return Json(listOfUsers);
                }
                catch (Exception ex)
                {
                    perfLogger.LogFailure(ex);
                    _logger.LogError(ex, "Error fetching all users - CorrelationId: {CorrelationId}", correlationId);
                    throw;
                }
            }
        }

        [HttpGet("{id}")]
        [Cacheable(durationInSeconds: 300)]
        public JsonResult GetUserById(string id)
        {
            var correlationId = GetCorrelationId();
            using (var perfLogger = new PerformanceLogger(_logger, "GetUserById", correlationId))
            {
                try
                {
                    _logger.LogInformation("Fetching user with id: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
                    List<User> listOfUsers = _repository.GetAllUsers();
                    User user = listOfUsers.Find(u => u.EmailId == id);
                    perfLogger.LogSuccess();
                    _logger.LogInformation("User retrieved: {UserEmail} - CorrelationId: {CorrelationId}", id, correlationId);
                    return Json(user);
                }
                catch (Exception ex)
                {
                    perfLogger.LogFailure(ex);
                    _logger.LogError(ex, "Error fetching user with id: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
                    throw;
                }
            }
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [InvalidateCache("user:*")]
        public JsonResult AddNewUser(User user)
        {
            var correlationId = GetCorrelationId();
            using (var perfLogger = new PerformanceLogger(_logger, "AddNewUser", correlationId))
            {
                try
                {
                    _logger.LogInformation("Adding new user with email: {UserEmail} - CorrelationId: {CorrelationId}", user?.EmailId ?? "unknown", correlationId);
                    var command = new UserMicroservices.CQRS.Commands.CreateUserCommand { User = user };
                    var dispatcher = HttpContext.RequestServices.GetService(typeof(SharedLibrary.CQRS.IDispatcher)) as SharedLibrary.CQRS.IDispatcher;
                    var result = dispatcher.Send<UserMicroservices.CQRS.Commands.CreateUserCommand, bool>(command).GetAwaiter().GetResult();
                    perfLogger.LogSuccess();
                    _logger.LogInformation("User added successfully with email: {UserEmail} - CorrelationId: {CorrelationId}", user?.EmailId, correlationId);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    perfLogger.LogFailure(ex);
                    _logger.LogError(ex, "Error adding new user - CorrelationId: {CorrelationId}", correlationId);
                    throw;
                }
            }
        }

        [HttpPut]
        [InvalidateCache("user:*")]
        public JsonResult UpdateUser(User user)
        {
            var correlationId = GetCorrelationId();
            using (var perfLogger = new PerformanceLogger(_logger, "UpdateUser", correlationId))
            {
                try
                {
                    _logger.LogInformation("Updating user with email: {UserEmail} - CorrelationId: {CorrelationId}", user?.EmailId ?? "unknown", correlationId);
                    int result = _repository.UpdateUserDetails(user);
                    perfLogger.LogSuccess();
                    _logger.LogInformation("User update completed with result: {UpdateResult} - CorrelationId: {CorrelationId}", result, correlationId);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    perfLogger.LogFailure(ex);
                    _logger.LogError(ex, "Error updating user - CorrelationId: {CorrelationId}", correlationId);
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        [InvalidateCache("user:*")]
        public JsonResult DeleteUser(string id)
        {
            var correlationId = GetCorrelationId();
            using (var perfLogger = new PerformanceLogger(_logger, "DeleteUser", correlationId))
            {
                try
                {
                    _logger.LogInformation("Deleting user with id: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
                    bool result = _repository.DeleteUser(id);
                    perfLogger.LogSuccess();
                    _logger.LogInformation("User deletion completed with result: {DeleteResult} - CorrelationId: {CorrelationId}", result, correlationId);
                    return Json(result);
                }
                catch (Exception ex)
                {
                    perfLogger.LogFailure(ex);
                    _logger.LogError(ex, "Error deleting user with id: {UserId} - CorrelationId: {CorrelationId}", id, correlationId);
                    throw;
                }
            }
        }
    }
}
