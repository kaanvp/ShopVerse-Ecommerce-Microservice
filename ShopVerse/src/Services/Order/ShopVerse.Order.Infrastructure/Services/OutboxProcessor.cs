using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShopVerse.Order.Infrastructure.Data;
using ShopVerse.Shared.Messaging;
using System.Text.Json;

namespace ShopVerse.Order.Infrastructure.Services
{
    /// <summary>
    /// Outbox tablosundaki işlenmemiş event'leri alıp RabbitMQ'ya gönderen background service'dir.
    /// </summary>
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

        public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxProcessor started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessages(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing outbox messages.");
                }

                await Task.Delay(_pollingInterval, stoppingToken);
            }

            _logger.LogInformation("OutboxProcessor stopped.");
        }

        private async Task ProcessOutboxMessages(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var messages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(50)
                .ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    var eventType = Type.GetType(message.EventType)
                        ?? AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .FirstOrDefault(t => t.FullName == message.EventType);
                    if (eventType is null)
                    {
                        _logger.LogWarning("Unknown event type: {EventType}", message.EventType);
                        message.ProcessedAt = DateTime.UtcNow;
                        continue;
                    }

                    var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType) as IDomainEvent;
                    if (domainEvent is null)
                    {
                        _logger.LogWarning("Failed to deserialize event: {EventType}", message.EventType);
                        message.ProcessedAt = DateTime.UtcNow;
                        continue;
                    }

                    await publishEndpoint.Publish(domainEvent, eventType, cancellationToken);
                    message.ProcessedAt = DateTime.UtcNow;

                    _logger.LogInformation("Outbox message published: {EventType} ({EventId})",
                        message.EventType, message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process outbox message {MessageId} ({EventType})",
                        message.Id, message.EventType);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
