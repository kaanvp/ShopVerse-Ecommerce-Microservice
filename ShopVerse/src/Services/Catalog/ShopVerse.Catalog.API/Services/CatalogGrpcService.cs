using Grpc.Core;
using ShopVerse.Catalog.Domain.Interface;
using ShopVerse.Catalog.Grpc;

namespace ShopVerse.Catalog.API.Services
{
    public class CatalogGrpcService : CatalogGrpc.CatalogGrpcBase
    {
        private readonly IProductRepository _productRepo;
        private readonly ILogger<CatalogGrpcService> _logger;

        public CatalogGrpcService(IProductRepository productRepo, ILogger<CatalogGrpcService> logger)
        {
            _productRepo = productRepo;
            _logger = logger;
        }

        public override async Task<ProductResponse> GetProductById(ProductRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out var productId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid product ID"));
            }

            var product = await _productRepo.GetByIdAsync(productId, context.CancellationToken);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
            }

            _logger.LogInformation("gRPC: Product {ProductId} fetched for basket validation", productId);

            return new ProductResponse
            {
                Id = product.Id.ToString(),
                Name = product.Name,
                Description = product.Description ?? "",
                Price = (double)product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId.ToString(),
                ImageUrl = product.ImageUrl ?? "",
                IsActive = product.IsActive
            };
        }
    }
}
