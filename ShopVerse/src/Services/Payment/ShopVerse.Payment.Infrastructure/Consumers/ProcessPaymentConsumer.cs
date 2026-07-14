using MassTransit;
using Microsoft.Extensions.Logging;
using ShopVerse.Payment.Application.Interfaces;
using ShopVerse.Payment.Domain.Entity;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging.Commands.Payment;
using ShopVerse.Shared.Messaging.Events;

namespace ShopVerse.Payment.Infrastructure.Consumers
{
    public class ProcessPaymentConsumer : IConsumer<ProcessPaymentMessage>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProcessPaymentConsumer> _logger;

        public ProcessPaymentConsumer(
            IPaymentRepository paymentRepository, 
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint,
            ILogger<ProcessPaymentConsumer> logger
            )
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProcessPaymentMessage> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing payment for OrderId: {OrderId}, Amount: {Amount}",
                message.OrderId, message.Amount);
            // Fake ödeme işlemi — %90 başarı
            var isSuccess = Random.Shared.NextDouble() < 0.90;
            var transactionId = isSuccess ? Guid.NewGuid().ToString("N")[..12].ToUpper() : null;
            // Payment kaydını oluştur
            var payment = new Payment.Domain.Entity.Payment
            {
                Id = Guid.NewGuid(),
                OrderId = message.OrderId,
                Amount = message.Amount,
                Status = isSuccess ? PaymentStatus.Completed : PaymentStatus.Failed,
                TransactionId = transactionId,
            };
            await _paymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();
            // Event publish
            if (isSuccess)
            {
                _logger.LogInformation("Payment SUCCESS for OrderId: {OrderId}, TransactionId: {TransactionId}",
                    message.OrderId, transactionId);

                await _publishEndpoint.Publish(new PaymentCompletedEvent
                {
                    OrderId = message.OrderId,
                    TransactionId = transactionId!,
                    Amount = message.Amount
                });

                // Bildirim talebi
                await _publishEndpoint.Publish(new NotificationRequestedEvent
                {
                    UserId = message.BuyerId,
                    Title = "Ödeme Başarılı",
                    Message = $"Siparişiniz için {message.Amount:C} tutarındaki ödeme başarıyla alındı.",
                    Type = "Payment"
                });
            }
            else
            {
                _logger.LogWarning("Payment FAILED for OrderId: {OrderId}", message.OrderId);
                await _publishEndpoint.Publish(new PaymentFailedEvent
                {
                    OrderId = message.OrderId,
                    Reason = "Ödeme işlemi sırasında bir hata oluştu."
                });
                await _publishEndpoint.Publish(new NotificationRequestedEvent
                {
                    UserId = message.BuyerId,
                    Title = "Ödeme Başarısız",
                    Message = $"Siparişiniz için ödeme işlemi başarısız oldu. Lütfen tekrar deneyin.",
                    Type = "Payment"
                });
            }
        }
    }
}
