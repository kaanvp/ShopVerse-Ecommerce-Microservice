using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;
using ShopVerse.Payment.Application.Interfaces;
using ShopVerse.Payment.Domain.Entity;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging.Events;

namespace ShopVerse.Payment.Infrastructure.Jobs
{
    public class PaymentTimeoutJob
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentTimeoutJob> _logger;

        public PaymentTimeoutJob(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint, ILogger<PaymentTimeoutJob> logger)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }
        [AutomaticRetry(Attempts = 0)] // Manuel yönet
        public async Task ProcessExpiredPayments()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-5);
            var expiredPayments = await _paymentRepository
                .GetPendingPaymentsOlderThanAsync(threshold);
            _logger.LogInformation("Found {Count} expired payments to process", expiredPayments.Count);
            
            foreach (var payment in expiredPayments)
            {
                payment.Status = PaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                await _publishEndpoint.Publish(new PaymentFailedEvent
                {
                    OrderId = payment.OrderId,
                    Reason = "Ödeme zaman aşımına uğradı (5 dk)."
                });

                _logger.LogWarning("Payment timed out for OrderId: {OrderId}", payment.OrderId);
            }
        }
    }
}
