using MongoDB.Driver;
using ShopVerse.Catalog.Domain.Entity;
using ShopVerse.Catalog.Domain.Interface;

namespace ShopVerse.Catalog.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<Category> _collection;

        public CategoryRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Category>("Categories");
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Category>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var filter = Builders<Category>.Filter.Empty;
            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Category entity, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(Category entity, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Category>.Filter.Eq(x => x.Id, entity.Id);
            await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(Category entity, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Category>.Filter.Eq(x => x.Id, entity.Id);
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }
    }
}
