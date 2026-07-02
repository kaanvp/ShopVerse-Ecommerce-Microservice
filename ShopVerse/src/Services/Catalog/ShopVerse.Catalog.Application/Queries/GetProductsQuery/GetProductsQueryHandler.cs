using MediatR;
using ShopVerse.Catalog.Application.DTOs;
using ShopVerse.Catalog.Application.Interfaces;
using ShopVerse.Catalog.Domain.Interface;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Queries.GetProductsQuery
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedResult<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICatalogCacheService _cacheService;

        public GetProductsQueryHandler(
            IProductRepository productRepository,
            ICatalogCacheService cacheService)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
        }

        public async Task<PaginatedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var filter = request.Filter;

            // Önce Redis'te list cache var mı kontrol et
            var cachedList = await _cacheService.GetListAsync();

            IReadOnlyList<ProductDto> productDtos;
            int totalCount;

            if (cachedList is not null)
            {
                // Cache varsa filtreleme + sayfalamayı memory'de yap
                var filtered = cachedList.AsEnumerable();

                if (filter.CategoryId.HasValue)
                    filtered = filtered.Where(x => x.CategoryId == filter.CategoryId.Value);

                if (filter.MinPrice.HasValue)
                    filtered = filtered.Where(x => x.Price >= filter.MinPrice.Value);

                if (filter.MaxPrice.HasValue)
                    filtered = filtered.Where(x => x.Price <= filter.MaxPrice.Value);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    filtered = filtered.Where(x =>
                        x.Name.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (x.Description?.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

                var filteredList = filtered.ToList();
                totalCount = filteredList.Count;

                productDtos = filteredList
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();
            }
            else
            {
                // Cache yoksa MongoDB'den getir
                var (items, count) = await _productRepository.GetFilteredAsync(
                    categoryId: filter.CategoryId,
                    minPrice: filter.MinPrice,
                    maxPrice: filter.MaxPrice,
                    searchTerm: filter.SearchTerm,
                    pageNumber: filter.PageNumber,
                    pageSize: filter.PageSize,
                    cancellationToken: cancellationToken);

                totalCount = count;

                productDtos = items
                    .Select(x => new ProductDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Price = x.Price,
                        Stock = x.Stock,
                        CategoryId = x.CategoryId,
                        ImageUrl = x.ImageUrl,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt
                    })
                    .ToList();

                // List cache'i doldur (sonraki sorgular için)
                var allItems = await _productRepository.GetAllAsync(cancellationToken);
                var allDtos = allItems
                    .Select(x => new ProductDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Price = x.Price,
                        Stock = x.Stock,
                        CategoryId = x.CategoryId,
                        ImageUrl = x.ImageUrl,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt
                    })
                    .ToList();

                await _cacheService.SetListAsync(allDtos);

                // Her bir ürünü de tekil cache'e yaz
                foreach (var dto in allDtos)
                {
                    await _cacheService.SetAsync(dto);
                }
            }

            return PaginatedResult<ProductDto>.Create(productDtos, totalCount, filter.PageNumber, filter.PageSize);
        }
    }
}
