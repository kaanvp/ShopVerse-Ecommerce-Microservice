using ShopVerse.Cargo.Domain.Entity;
using ShopVerse.Cargo.Domain.Enums;
using static ShopVerse.Cargo.Infrastructure.Data.ShipmentRepository;

namespace ShopVerse.Cargo.Infrastructure.Data
{
    internal static class ShipmentMapper
    {
        public static Shipment FromRow(ShipmentRow row)
        {
            return Shipment.Reconstruct(
                id: row.id,
                orderId: row.order_id,
                buyerId: row.buyer_id,
                trackingNumber: row.tracking_number,
                status: (ShipmentStatus)row.status,
                shippingAddress: row.shipping_address,
                city: row.city,
                district: row.district,
                zipCode: row.zip_code,
                estimatedDelivery: row.estimated_delivery,
                createdAt: row.created_at
            );
        }
    }
}
