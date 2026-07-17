using MassTransit;
using ShopVerse.Shared.Messaging.Commands.Cargo;
using ShopVerse.Shared.Messaging.Commands.Catalog;
using ShopVerse.Shared.Messaging.Commands.Payment;
using ShopVerse.Shared.Messaging.Events;

namespace ShopVerse.Order.Infrastructure.Sagas
{
#pragma warning disable CS8618 // Properties initialized by MassTransit framework
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State OrderCreated { get; private set; }
        public State StockReserved { get; private set; }
        public State PaymentCompleted { get; private set; }
        public State PaymentFailed { get; private set; }
        public State OrderCancelled { get; private set; }

        public Event<OrderCreatedEvent> OrderCreatedEvent { get; private set; }
        public Event<StockReservedEvent> StockReservedEvent { get; private set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; private set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            // Correlation ID: OrderCreatedEvent.OrderId ile saga instance'ını eşle
            Event(() => OrderCreatedEvent, x =>
                x.CorrelateById(context => context.Message.OrderId));

            Event(() => StockReservedEvent, x =>
                x.CorrelateById(context => context.Message.OrderId));

            Event(() => PaymentCompletedEvent, x =>
                x.CorrelateById(context => context.Message.OrderId));

            Event(() => PaymentFailedEvent, x =>
                x.CorrelateById(context => context.Message.OrderId));

            // Başlangıç: OrderCreated → ReserveStock
            Initially(
                When(OrderCreatedEvent)
                    .Then(context =>
                    {
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.BuyerId = context.Message.BuyerId;
                        context.Saga.TotalAmount = context.Message.TotalAmount;
                        context.Saga.PaymentMethod = "CreditCard";
                        context.Saga.ShippingAddressFullName = context.Message.ShippingAddressFullName;
                        context.Saga.ShippingCity = context.Message.ShippingCity;
                        context.Saga.ShippingDistrict = context.Message.ShippingDistrict;
                        context.Saga.ShippingAddressLine = context.Message.ShippingAddressLine;
                        context.Saga.ShippingZipCode = context.Message.ShippingZipCode;
                    })
                    .SendAsync(context => context.Init<ReserveStockMessage>(new ReserveStockMessage
                    {
                        OrderId = context.Message.OrderId,
                        Items = context.Message.Items
                            .Select(i => new StockItemDto(i.ProductId, i.Quantity))
                            .ToList()
                    }))
                    .TransitionTo(OrderCreated)
            );

            // OrderCreated durumunda StockReservedEvent bekle
            During(OrderCreated,
                When(StockReservedEvent)
                    .If(context => context.Message.IsSuccess,
                        then => then
                            .SendAsync(context => context.Init<ProcessPaymentMessage>(new ProcessPaymentMessage
                            {
                                OrderId = context.Message.OrderId,
                                BuyerId = context.Saga.BuyerId,
                                Amount = context.Saga.TotalAmount,
                                PaymentMethod = "CreditCard"
                            }))
                            .TransitionTo(StockReserved)
                    )
                    .If(context => !context.Message.IsSuccess,
                        then => then
                            .TransitionTo(PaymentFailed)
                    )
            );

            // StockReserved durumunda PaymentCompleted veya PaymentFailed bekle
            During(StockReserved,
                When(PaymentCompletedEvent)
                    .SendAsync(context => context.Init<CreateShipmentMessage>(new CreateShipmentMessage
                    {
                        OrderId = context.Message.OrderId,
                        BuyerId = context.Saga.BuyerId,
                        ShippingAddress = context.Saga.ShippingAddressLine,
                        City = context.Saga.ShippingCity,
                        District = context.Saga.ShippingDistrict,
                        ZipCode = context.Saga.ShippingZipCode
                    }))
                    .TransitionTo(PaymentCompleted),
                When(PaymentFailedEvent)
                    .SendAsync(context => context.Init<ReleaseStockMessage>(new ReleaseStockMessage
                    {
                        OrderId = context.Message.OrderId,
                        Items = new List<StockItemDto>()
                    }))
                    .TransitionTo(PaymentFailed)
            );

            // PaymentCompleted sonrası iptal/fail gibi durumlar boş
            During(PaymentCompleted,
                When(PaymentFailedEvent)
                    .SendAsync(context => context.Init<ReleaseStockMessage>(new ReleaseStockMessage
                    {
                        OrderId = context.Message.OrderId,
                        Items = new List<StockItemDto>()
                    }))
                    .TransitionTo(PaymentFailed)
            );
        }
    }
}
#pragma warning restore CS8618
