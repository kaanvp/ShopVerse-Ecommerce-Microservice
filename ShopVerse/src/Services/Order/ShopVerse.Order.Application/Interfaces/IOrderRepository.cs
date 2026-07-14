using ShopVerse.Order.Domain.Entity;
using ShopVerse.Shared.Core;

namespace ShopVerse.Order.Application.Interfaces
{
    public interface IOrderRepository : IRepository<Domain.Entity.Order>
    {
        Task<IReadOnlyList<Domain.Entity.Order>> GetOrdersByBuyerIdAsync(Guid buyerId, CancellationToken cancellationToken = default);
        Task<Domain.Entity.Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    }
}
