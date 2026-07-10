using System.Threading;
using System.Threading.Tasks;
using SharedLibrary.CQRS;
using UserMicroservices.CQRS.Commands;
using UserMicroservices.Repository;

namespace UserMicroservices.CQRS.Handlers
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, bool>
    {
        private readonly UserRepository _repository;

        public CreateUserCommandHandler(UserRepository repository)
        {
            _repository = repository;
        }

        public Task<bool> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
        {
            var result = _repository.AddNewUser(request.User);
            return Task.FromResult(result);
        }
    }
}
