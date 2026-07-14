using Microsoft.EntityFrameworkCore;
using ShopVerse.Order.Application.Interfaces;
using ShopVerse.Shared.Core;

namespace ShopVerse.Order.Infrastructure.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Entity.Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Domain.Entity.Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Domain.Entity.Order entity, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(Domain.Entity.Order entity, CancellationToken cancellationToken = default)
        {
            _context.Orders.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Domain.Entity.Order entity, CancellationToken cancellationToken = default)
        {
            _context.Orders.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<Domain.Entity.Order>> GetOrdersByBuyerIdAsync(Guid buyerId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.BuyerId == buyerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Domain.Entity.Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }
    }
}
