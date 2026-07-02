using MongoDB.Driver;
using ShopVerse.Catalog.Domain.Entity;
using ShopVerse.Catalog.Domain.Interface;

namespace ShopVerse.Catalog.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _collection;

        public ProductRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Product>("Products");
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var filter = Builders<Product>.Filter.Empty;
            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        private FilterDefinition<Product> BuildFilter(
            Guid? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? searchTerm = null)
        {
            var filterBuilder = Builders<Product>.Filter;
            var filters = new List<FilterDefinition<Product>>();

            if (categoryId.HasValue)
                filters.Add(filterBuilder.Eq(x => x.CategoryId, categoryId.Value));

            if (minPrice.HasValue)
                filters.Add(filterBuilder.Gte(x => x.Price, minPrice.Value));

            if (maxPrice.HasValue)
                filters.Add(filterBuilder.Lte(x => x.Price, maxPrice.Value));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchPattern = searchTerm.ToLowerInvariant();
                var nameFilter = filterBuilder.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(searchPattern, "i"));
                var descFilter = filterBuilder.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(searchPattern, "i"));
                filters.Add(filterBuilder.Or(nameFilter, descFilter));
            }

            return filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;
        }

        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetFilteredAsync(
            Guid? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var filter = BuildFilter(categoryId, minPrice, maxPrice, searchTerm);

            var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            var items = await _collection
                .Find(filter)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return (items, (int)totalCount);
        }

        public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, entity.Id);
            await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, entity.Id);
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }

        public async Task UpdateStockAsync(Guid id, int quantity, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
            var update = Builders<Product>.Update.Set(x => x.Stock, quantity);
            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }
    }
}
