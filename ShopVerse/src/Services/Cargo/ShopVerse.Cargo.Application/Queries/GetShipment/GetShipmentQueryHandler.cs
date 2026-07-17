using MediatR;
using ShopVerse.Cargo.Application.DTOs;
using ShopVerse.Cargo.Application.Interfaces;
using ShopVerse.Shared.Core;

namespace ShopVerse.Cargo.Application.Queries.GetShipment
{
    public class GetShipmentQueryHandler : IRequestHandler<GetShipmentQuery, Result<ShipmentDto>>
    {
        private readonly IShipmentRepository _repository;

        public GetShipmentQueryHandler(IShipmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ShipmentDto>> Handle(GetShipmentQuery request, CancellationToken cancellationToken)
        {
            var shipment = await _repository.GetByTrackingNumberAsync(request.TrackingNumber, cancellationToken);
            if (shipment == null)
                return Result<ShipmentDto>.Failure("Shipment not found.", 404);

            return Result<ShipmentDto>.Success(new ShipmentDto
            {
                Id = shipment.Id,
                OrderId = shipment.OrderId,
                TrackingNumber = shipment.TrackingNumber,
                Status = shipment.Status.ToString(),
                ShippingAddress = shipment.ShippingAddress,
                City = shipment.City,
                District = shipment.District,
                ZipCode = shipment.ZipCode,
                EstimatedDelivery = shipment.EstimatedDelivery,
                CreatedAt = shipment.CreatedAt
            });
        }
    }
}
