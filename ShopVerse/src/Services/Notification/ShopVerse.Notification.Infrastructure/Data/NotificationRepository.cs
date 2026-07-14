using MongoDB.Driver;
using ShopVerse.Notification.Application.Interfaces;
using ShopVerse.Notification.Domain.Entity;
using ShopVerse.Notification.Infrastructure.Settings;

namespace ShopVerse.Notification.Infrastructure.Data
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification.Domain.Entity.Notification> _collection;

        public NotificationRepository(IMongoDatabase database, MongoDbSettings settings)
        {
            _collection = database.GetCollection<Notification.Domain.Entity.Notification>(
                settings.NotificationsCollectionName);
        }

        public async Task<Notification.Domain.Entity.Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Notification.Domain.Entity.Notification>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Notification.Domain.Entity.Notification>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _collection.Find(_ => true).ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Notification.Domain.Entity.Notification entity, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        }

        public Task UpdateAsync(Notification.Domain.Entity.Notification entity, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Notification.Domain.Entity.Notification>.Filter.Eq(x => x.Id, entity.Id);
            return _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        }

        public Task DeleteAsync(Notification.Domain.Entity.Notification entity, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Notification.Domain.Entity.Notification>.Filter.Eq(x => x.Id, entity.Id);
            return _collection.DeleteOneAsync(filter, cancellationToken);
        }

        public async Task<IReadOnlyList<Notification.Domain.Entity.Notification>> GetByUserIdAsync(
            Guid userId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Notification.Domain.Entity.Notification>.Filter.Eq(x => x.UserId, userId);
            return await _collection
                .Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Notification.Domain.Entity.Notification>> GetUnreadByUserIdAsync(
            Guid userId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<Notification.Domain.Entity.Notification>.Filter.And(
                Builders<Notification.Domain.Entity.Notification>.Filter.Eq(x => x.UserId, userId),
                Builders<Notification.Domain.Entity.Notification>.Filter.Eq(x => x.IsRead, false));
            return await _collection
                .Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
