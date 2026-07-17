using ShopVerse.Cargo.Domain.Enums;
using ShopVerse.Shared.Core;

namespace ShopVerse.Cargo.Domain.Entity
{
    public class Shipment : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public Guid BuyerId { get; private set; }
        public string TrackingNumber { get; private set; } = string.Empty;
        public ShipmentStatus Status { get; private set; }
        public string ShippingAddress { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string District { get; private set; } = string.Empty;
        public string ZipCode { get; private set; } = string.Empty;
        public DateTime EstimatedDelivery { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Shipment() { }

        public static Shipment Create(
            Guid orderId,
            Guid buyerId,
            string shippingAddress,
            string city,
            string district,
            string zipCode)
        {
            return new Shipment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                BuyerId = buyerId,
                TrackingNumber = GenerateTrackingNumber(),
                Status = ShipmentStatus.Preparing,
                ShippingAddress = shippingAddress,
                City = city,
                District = district,
                ZipCode = zipCode,
                EstimatedDelivery = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateStatus(ShipmentStatus newStatus)
        {
            Status = newStatus;
        }

        // Used by infrastructure layer to rehydrate entity from persistence
        public static Shipment Reconstruct(
            Guid id,
            Guid orderId,
            Guid buyerId,
            string trackingNumber,
            ShipmentStatus status,
            string shippingAddress,
            string city,
            string district,
            string zipCode,
            DateTime estimatedDelivery,
            DateTime createdAt)
        {
            return new Shipment
            {
                Id = id,
                OrderId = orderId,
                BuyerId = buyerId,
                TrackingNumber = trackingNumber,
                Status = status,
                ShippingAddress = shippingAddress,
                City = city,
                District = district,
                ZipCode = zipCode,
                EstimatedDelivery = estimatedDelivery,
                CreatedAt = createdAt
            };
        }

        private static string GenerateTrackingNumber()
        {
            var prefix = "SV";
            var timestamp = DateTime.UtcNow.ToString("yyMMddHHmm");
            var random = new Random().Next(1000, 9999);
            return $"{prefix}{timestamp}{random}";
        }
    }
}
