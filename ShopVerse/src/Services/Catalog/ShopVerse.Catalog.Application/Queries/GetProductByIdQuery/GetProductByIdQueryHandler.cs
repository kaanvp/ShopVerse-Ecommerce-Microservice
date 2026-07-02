using MediatR;
using ShopVerse.Catalog.Application.DTOs;
using ShopVerse.Catalog.Application.Interfaces;
using ShopVerse.Catalog.Domain.Interface;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Queries.GetProductByIdQuery
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICatalogCacheService _cacheService;

        public GetProductByIdQueryHandler(
            IProductRepository productRepository,
            ICatalogCacheService cacheService)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
        }
        // Redis cache aside pattern uygulanıyor.
        public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            // Önce Redis cache'e bak
            var cachedProduct = await _cacheService.GetAsync(request.Id);
            if (cachedProduct is not null)
                return Result<ProductDto>.Success(cachedProduct);

            // Cache'te yoksa MongoDB'den (repository) getir
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null)
                return Result<ProductDto>.Failure("Ürün bulunamadı.", 404);

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt
            };

            // Cache'e yaz
            await _cacheService.SetAsync(productDto);

            return Result<ProductDto>.Success(productDto);
        }
    }
}
