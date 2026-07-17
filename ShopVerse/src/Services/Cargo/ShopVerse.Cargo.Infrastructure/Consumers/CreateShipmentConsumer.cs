using MassTransit;
using Microsoft.Extensions.Logging;
using ShopVerse.Cargo.Application.Interfaces;
using ShopVerse.Cargo.Domain.Entity;
using ShopVerse.Shared.Messaging.Commands.Cargo;
using ShopVerse.Shared.Messaging.Events;

namespace ShopVerse.Cargo.Infrastructure.Consumers
{
    public class CreateShipmentConsumer : IConsumer<CreateShipmentMessage>
    {
        private readonly IShipmentRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CreateShipmentConsumer> _logger;

        public CreateShipmentConsumer(
            IShipmentRepository repository,
            IPublishEndpoint publishEndpoint,
            ILogger<CreateShipmentConsumer> logger)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CreateShipmentMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Creating shipment for Order {OrderId}, Buyer {BuyerId}", message.OrderId, message.BuyerId);

            // Idempotency: check if shipment already exists for this order
            var existing = await _repository.GetByOrderIdAsync(message.OrderId, context.CancellationToken);
            if (existing != null)
            {
                _logger.LogWarning("Shipment already exists for Order {OrderId}. Skipping creation.", message.OrderId);
                return;
            }

            var shipment = Shipment.Create(
                orderId: message.OrderId,
                buyerId: message.BuyerId,
                shippingAddress: message.ShippingAddress,
                city: message.City,
                district: message.District,
                zipCode: message.ZipCode);

            await _repository.AddAsync(shipment, context.CancellationToken);

            _logger.LogInformation("Shipment {TrackingNumber} created for Order {OrderId}",
                shipment.TrackingNumber, shipment.OrderId);

            // Publish CargoShippedEvent → Notification service will pick this up
            await _publishEndpoint.Publish(new CargoShippedEvent
            {
                OrderId = shipment.OrderId,
                TrackingNumber = shipment.TrackingNumber,
                EstimatedDeliveryDate = shipment.EstimatedDelivery
            }, context.CancellationToken);
        }
    }
}
