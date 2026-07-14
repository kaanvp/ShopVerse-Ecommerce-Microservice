using ShopVerse.Shared.Core;

namespace ShopVerse.Notification.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        // MongoDB'de her repository metodu kendi işlemini anında gerçekleştirir.
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }
}
