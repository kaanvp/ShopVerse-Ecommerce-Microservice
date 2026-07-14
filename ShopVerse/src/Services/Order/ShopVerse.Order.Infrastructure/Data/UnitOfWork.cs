using ShopVerse.Order.Domain.Entity;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging;
using System.Text.Json;

namespace ShopVerse.Order.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _context;

        public UnitOfWork(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Domain event'leri topla ve Outbox'a yaz
            var domainEventEntries = _context.ChangeTracker
                .Entries<Domain.Entity.Order>()
                .Where(e => e.Entity.DomainEvents.Any())
                .ToList();

            foreach (var entry in domainEventEntries)
            {
                var order = entry.Entity;
                var events = order.DomainEvents.ToList();
                order.ClearDomainEvents();

                foreach (var domainEvent in events)
                {
                    _context.OutboxMessages.Add(new OutboxMessage
                    {
                        Id = domainEvent.EventId,
                        EventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                        Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
