using ShopVerse.Cargo.Domain.Enums;

namespace ShopVerse.Cargo.Application.DTOs
{
    public class ShipmentDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public DateTime EstimatedDelivery { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
