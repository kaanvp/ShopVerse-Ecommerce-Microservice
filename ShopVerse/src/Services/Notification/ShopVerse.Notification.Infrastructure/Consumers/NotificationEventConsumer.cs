using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShopVerse.Notification.Application.Interfaces;
using ShopVerse.Notification.Domain.Entity;
using ShopVerse.Notification.Infrastructure.Hubs;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Notification.Infrastructure.Consumers
{
    public class NotificationEventConsumer : IConsumer<NotificationRequestedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationEventConsumer> _logger;

        public NotificationEventConsumer(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationEventConsumer> logger)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<NotificationRequestedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Notification for User {UserId}: {Title}", message.UserId, message.Title);

            // DB'ye kaydet
            var notification = new Notification.Domain.Entity.Notification
            {
                Id = Guid.NewGuid(),
                UserId = message.UserId,
                Title = message.Title,
                Message = message.Message,
                Type = message.Type,
                IsRead = false,
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // SignalR ile anlık bildirim gönder
            await _hubContext.Clients.Group($"user-{message.UserId}")
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
