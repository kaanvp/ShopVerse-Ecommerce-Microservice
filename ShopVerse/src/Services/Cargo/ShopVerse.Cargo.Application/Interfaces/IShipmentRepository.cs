using ShopVerse.Cargo.Domain.Entity;

namespace ShopVerse.Cargo.Application.Interfaces
{
    public interface IShipmentRepository
    {
        Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);
        Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
        Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default);
    }
}
