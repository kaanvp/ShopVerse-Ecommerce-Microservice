using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShopVerse.Notification.Application.Interfaces;
using ShopVerse.Notification.Infrastructure.Hubs;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging.Events;

namespace ShopVerse.Notification.Infrastructure.Consumers
{
    public class CargoShippedConsumer : IConsumer<CargoShippedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<CargoShippedConsumer> _logger;

        public CargoShippedConsumer(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext,
            ILogger<CargoShippedConsumer> logger)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<CargoShippedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Cargo shipped notification for Order {OrderId}, Tracking: {TrackingNumber}",
                message.OrderId, message.TrackingNumber);

            _logger.LogDebug("CargoShippedEvent received for OrderId: {OrderId}. " +
                "Tracking: {TrackingNumber}, Estimated: {EstimatedDate}",
                message.OrderId, message.TrackingNumber, message.EstimatedDeliveryDate);

            return Task.CompletedTask;
        }
    }
}
