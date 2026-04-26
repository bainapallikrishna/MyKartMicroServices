using Grpc.Core;
using MyKart.Contracts.Category;
using CategoryMicroservices.Repository;

namespace CategoryMicroservices.Grpc;

public sealed class CategoryGrpcService : CategoryGrpc.CategoryGrpcBase
{
    private readonly CategoryRepository _repo;

    public CategoryGrpcService(CategoryRepository repo)
    {
        _repo = repo;
    }

    public override Task<GetCategoryReply> GetCategoryById(GetCategoryByIdRequest request, ServerCallContext context)
    {
        var category = _repo.GetAllCategories().Find(c => c.CategoryId == (byte)request.CategoryId);

        return Task.FromResult(new GetCategoryReply
        {
            CategoryId = request.CategoryId,
            CategoryName = category?.CategoryName ?? string.Empty
        });
    }
}

