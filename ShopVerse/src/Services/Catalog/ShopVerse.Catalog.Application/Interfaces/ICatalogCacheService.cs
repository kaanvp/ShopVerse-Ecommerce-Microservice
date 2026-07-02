using ShopVerse.Catalog.Application.DTOs;

namespace ShopVerse.Catalog.Application.Interfaces
{
    public interface ICatalogCacheService
    {
        Task<ProductDto?> GetAsync(Guid id);
        Task SetAsync(ProductDto product);
        Task RemoveAsync(Guid id);
        Task<IReadOnlyList<ProductDto>?> GetListAsync();
        Task SetListAsync(IReadOnlyList<ProductDto> products);
        Task RemoveListAsync();
    }
}
