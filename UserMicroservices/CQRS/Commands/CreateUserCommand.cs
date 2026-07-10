using SharedLibrary.CQRS;
using UserMicroservices.Models;

namespace UserMicroservices.CQRS.Commands
{
    public class CreateUserCommand : ICommand<bool>
    {
        public User User { get; set; }
    }
}
