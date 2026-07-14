using ShopVerse.Shared.Core;

namespace ShopVerse.Order.Domain.Entity
{
    public class OrderItem : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }

        private OrderItem() { }

        public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
        {
            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }

        public decimal GetTotalPrice() => UnitPrice * Quantity;

        public void UpdateQuantity(int quantity)
        {
            Quantity = quantity;
        }
    }
}
