using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShopVerse.Notification.Application.Interfaces;
using ShopVerse.Notification.Infrastructure.Hubs;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging.Events;

namespace ShopVerse.Notification.Infrastructure.Consumers
{
    public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<PaymentCompletedConsumer> _logger;

        // Not: PaymentCompletedEvent'te BuyerId yok, OrderId üzerinden ilerliyoruz.
        // Bildirim bir kullanıcıya gitmesi gerekiyorsa OrderId'den BuyerId çözülmeli.
        // Şimdilik OrderId'yi UserId olarak kullanıyoruz — Order Saga'dan gelen event'e göre düzeltilecek.

        public PaymentCompletedConsumer(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext,
            ILogger<PaymentCompletedConsumer> logger)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Payment completed notification for Order {OrderId}, Transaction {TransactionId}",
                message.OrderId, message.TransactionId);

            _logger.LogDebug("PaymentCompletedEvent received for OrderId: {OrderId}. " +
                "Notification already sent via ProcessPaymentConsumer.", message.OrderId);

            return Task.CompletedTask;
        }
    }
}
