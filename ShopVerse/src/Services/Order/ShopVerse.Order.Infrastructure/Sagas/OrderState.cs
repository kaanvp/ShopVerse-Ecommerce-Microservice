using MassTransit;

namespace ShopVerse.Order.Infrastructure.Sagas
{
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = string.Empty;

        public Guid OrderId { get; set; }
        public Guid BuyerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }

        // Shipping address for cargo step
        public string ShippingAddressFullName { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingDistrict { get; set; } = string.Empty;
        public string ShippingAddressLine { get; set; } = string.Empty;
        public string ShippingZipCode { get; set; } = string.Empty;

        // RowVersion for optimistic concurrency
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
