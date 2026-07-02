using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Infrastructure.Repositories
{
    /// <summary>
    /// MongoDB için UnitOfWork implementasyonu.
    /// MongoDB'de tek doküman işlemleri atomic olduğu için
    /// SaveChangesAsync herhangi bir işlem yapmaz (her repository metodu kendi işlemini anında yapar).
    /// Gerektiğinde replica set üzerinden transaction desteği eklenebilir.
    /// </summary>
    public class MongoUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // MongoDB'de her repository metodu (InsertOne, ReplaceOne, DeleteOne)
            // kendi işlemini anında gerçekleştirir. Bu nedenle burada ek bir işlem yapılmaz.
            return Task.FromResult(1);
        }
    }
}
