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

        public async Task Consume(ConsumeContext<CargoShippedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Cargo shipped notification for Order {OrderId}, Tracking: {TrackingNumber}",
                message.OrderId, message.TrackingNumber);

            // NOTE: CargoShippedEvent does not carry BuyerId.
            // We store the notification keyed by OrderId as UserId placeholder.
            // A full implementation would resolve BuyerId from the Order service.
            var notification = new Domain.Entity.Notification
            {
                Id = Guid.NewGuid(),
                UserId = message.OrderId,  // OrderId used as fallback until BuyerId enrichment
                Title = "Kargonuz Yola Çıktı",
                Message = $"Siparişiniz kargoya verildi. Takip No: {message.TrackingNumber}. Tahmini teslimat: {message.EstimatedDeliveryDate:dd.MM.yyyy}.",
                Type = "Cargo",
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            await _hubContext.Clients.Group($"user-{message.OrderId}")
                .SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Title,
                    notification.Message,
                    notification.Type,
                    notification.CreatedAt,
                    notification.IsRead
                });
        }
    }
}
