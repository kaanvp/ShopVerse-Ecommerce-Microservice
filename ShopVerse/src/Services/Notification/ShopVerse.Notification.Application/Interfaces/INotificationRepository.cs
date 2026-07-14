using ShopVerse.Shared.Core;

namespace ShopVerse.Notification.Application.Interfaces
{
    public interface INotificationRepository : IRepository<Domain.Entity.Notification>
    {
        Task<IReadOnlyList<Domain.Entity.Notification>> GetByUserIdAsync(
            Guid userId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Domain.Entity.Notification>> GetUnreadByUserIdAsync(
            Guid userId, CancellationToken cancellationToken = default);
    }
}
