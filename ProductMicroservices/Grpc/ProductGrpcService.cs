using Grpc.Core;
using MyKart.Contracts.Product;
using ProductMicroservices.Repository;

namespace ProductMicroservices.Grpc;

public sealed class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly ProductRepository _repo;

    public ProductGrpcService(ProductRepository repo)
    {
        _repo = repo;
    }

    public override async Task<GetPriceResponse> GetPrice(GetPriceRequest request, ServerCallContext context)
    {
        var price = await _repo.GetPrice(request.ProductId);
        return new GetPriceResponse { Price = (double)price };
    }

    public override async Task<UpdateQuantityResponse> UpdateQuantity(UpdateQuantityRequest request, ServerCallContext context)
    {
        var result = await _repo.UpdateQuantity(request.ProductId, request.QuantityPurchased);
        return new UpdateQuantityResponse { Result = result };
    }
}

