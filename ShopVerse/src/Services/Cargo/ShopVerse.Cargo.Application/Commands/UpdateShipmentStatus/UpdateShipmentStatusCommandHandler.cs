using MassTransit;
using MediatR;
using ShopVerse.Cargo.Application.Interfaces;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging.Events;

namespace ShopVerse.Cargo.Application.Commands.UpdateShipmentStatus
{
    public class UpdateShipmentStatusCommandHandler : IRequestHandler<UpdateShipmentStatusCommand, Result<Unit>>
    {
        private readonly IShipmentRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateShipmentStatusCommandHandler(
            IShipmentRepository repository,
            IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result<Unit>> Handle(UpdateShipmentStatusCommand request, CancellationToken cancellationToken)
        {
            var shipment = await _repository.GetByIdAsync(request.ShipmentId, cancellationToken);
            if (shipment == null)
                return Result<Unit>.Failure("Shipment not found.", 404);

            shipment.UpdateStatus(request.NewStatus);
            await _repository.UpdateAsync(shipment, cancellationToken);

            // Publish status updated event → Notification service
            await _publishEndpoint.Publish(new CargoStatusUpdatedEvent
            {
                OrderId = shipment.OrderId,
                BuyerId = shipment.BuyerId,
                TrackingNumber = shipment.TrackingNumber,
                NewStatus = request.NewStatus.ToString()
            }, cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
