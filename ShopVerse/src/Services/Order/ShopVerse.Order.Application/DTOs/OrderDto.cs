using ShopVerse.Order.Domain.Enums;

namespace ShopVerse.Order.Application.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public decimal TotalPrice { get; set; }
        public ShippingAddressDto? ShippingAddress { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class ShippingAddressDto
    {
        public string FullName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }
}
