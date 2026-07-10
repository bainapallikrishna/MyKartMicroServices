using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedLibrary.CQRS;
using UserMicroservices.CQRS.Queries;
using UserMicroservices.Models;
using UserMicroservices.Repository;

namespace UserMicroservices.CQRS.Handlers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<User>>
    {
        private readonly UserRepository _repository;

        public GetAllUsersQueryHandler(UserRepository repository)
        {
            _repository = repository;
        }

        public Task<List<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken = default)
        {
            var list = _repository.GetAllUsers();
            return Task.FromResult(list);
        }
    }
}
