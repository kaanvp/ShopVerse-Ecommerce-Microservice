using ShopVerse.Order.Domain.Enums;
using ShopVerse.Order.Domain.ValueObjects;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging;

namespace ShopVerse.Order.Domain.Entity
{
    public class Order : AuditableEntity, IAggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        private readonly List<OrderItem> _orderItems = new();

        public Guid BuyerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public ShippingAddress? ShippingAddress { get; private set; }
        public decimal TotalPrice { get; private set; }
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        private Order() { }

        public Order(Guid buyerId, ShippingAddress shippingAddress)
        {
            Id = Guid.NewGuid();
            BuyerId = buyerId;
            Status = OrderStatus.Created;
            ShippingAddress = shippingAddress;
            TotalPrice = 0;
        }

        public void AddOrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
        {
            var existingItem = _orderItems.FirstOrDefault(x => x.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                _orderItems.Add(new OrderItem(productId, productName, unitPrice, quantity));
            }

            RecalculateTotalPrice();
        }

        public void RemoveOrderItem(Guid productId)
        {
            var item = _orderItems.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                _orderItems.Remove(item);
                RecalculateTotalPrice();
            }
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
        }

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        private void RecalculateTotalPrice()
        {
            TotalPrice = _orderItems.Sum(x => x.GetTotalPrice());
        }
    }
}
