using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShopVerse.Notification.Application.Interfaces;
using ShopVerse.Notification.Infrastructure.Hubs;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging.Events;

namespace ShopVerse.Notification.Infrastructure.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext,
            ILogger<OrderCreatedConsumer> logger)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Order created notification for User {UserId}, Order {OrderId}",
                message.BuyerId, message.OrderId);

            var notification = new Domain.Entity.Notification
            {
                Id = Guid.NewGuid(),
                UserId = message.BuyerId,
                Title = "Siparişiniz Alındı",
                Message = $"Siparişiniz (No: {message.OrderId.ToString("N")[..8].ToUpper()}) başarıyla oluşturuldu. Toplam tutar: {message.TotalAmount:C}.",
                Type = "Order",
                IsRead = false,
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

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
