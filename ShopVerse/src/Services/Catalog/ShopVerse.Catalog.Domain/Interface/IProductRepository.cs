using ShopVerse.Catalog.Domain.Entity;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Domain.Interface
{
    public interface IProductRepository : IRepository<Product>
    {
        Task UpdateStockAsync(Guid id, int quantity, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
            Guid? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
    }
}
