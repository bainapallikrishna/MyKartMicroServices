using SharedLibrary.CQRS;
using UserMicroservices.Models;
using System.Collections.Generic;

namespace UserMicroservices.CQRS.Queries
{
    public class GetAllUsersQuery : IQuery<List<User>>
    {
    }
}
