using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShopVerse.Notification.Application.Interfaces;
using ShopVerse.Notification.Infrastructure.Hubs;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging.Events;

namespace ShopVerse.Notification.Infrastructure.Consumers
{
    public class CargoStatusUpdatedConsumer : IConsumer<CargoStatusUpdatedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<CargoStatusUpdatedConsumer> _logger;

        public CargoStatusUpdatedConsumer(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext,
            ILogger<CargoStatusUpdatedConsumer> logger)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CargoStatusUpdatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Cargo status updated for Order {OrderId}, Buyer {BuyerId}, Status: {Status}",
                message.OrderId, message.BuyerId, message.NewStatus);

            var notification = new Domain.Entity.Notification
            {
                Id = Guid.NewGuid(),
                UserId = message.BuyerId,
                Title = "Kargo Durumu Güncellendi",
                Message = $"Siparişinizin kargo durumu güncellendi: {message.NewStatus}. Takip No: {message.TrackingNumber}.",
                Type = "Cargo",
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // Send real-time SignalR notification to the buyer
            await _hubContext.Clients.Group($"user-{message.BuyerId}")
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
