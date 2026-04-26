using Grpc.Core;
using MyKart.Contracts.User;
using UserMicroservices.Repository;

namespace UserMicroservices.Grpc;

public sealed class UserGrpcService : UserGrpc.UserGrpcBase
{
    private readonly UserRepository _repo;

    public UserGrpcService(UserRepository repo)
    {
        _repo = repo;
    }

    public override Task<UserExistsReply> UserExists(UserExistsRequest request, ServerCallContext context)
    {
        var exists = _repo.GetAllUsers().Any(u => u.EmailId == request.EmailId);
        return Task.FromResult(new UserExistsReply { Exists = exists });
    }
}

